using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.StudyPlan;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class StudyPlanService : IStudyPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeminiAiService _geminiAiService;
        private readonly IMapper _mapper;
        private readonly ILogger<StudyPlanService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public StudyPlanService(
            IUnitOfWork unitOfWork,
            IGeminiAiService geminiAiService,
            IMapper mapper,
            ILogger<StudyPlanService> logger)
        {
            _unitOfWork = unitOfWork;
            _geminiAiService = geminiAiService;
            _mapper = mapper;
            _logger = logger;
        }

        // =========================================================
        // 1. GENERATE WEEKLY PLAN
        // =========================================================
        public async Task<ResponseDto> GenerateWeeklyPlanAsync(GeneratePlanDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            try
            {
                // 1. Phân tích kết quả học tập (có thể trả về null nếu không có history)
                var performanceResult = await AnalyzeSubjectPerformance(userId, dto.LookbackDays);

                // Nếu không có dữ liệu, tạo performance mặc định (dựa trên focusSubjects nếu có)
                if (performanceResult == null ||
                    ((performanceResult.WeakSubjects == null || !performanceResult.WeakSubjects.Any()) &&
                     (performanceResult.StrongSubjects == null || !performanceResult.StrongSubjects.Any())))
                {
                    performanceResult = CreateDefaultPerformance(dto.FocusSubjects);
                }

                // 2. Chuẩn bị preferences JSON
                var weekStart = dto.WeekStart ?? GetNextMonday();
                var preferencesJson = JsonSerializer.Serialize(new
                {
                    dailyStudyHours = dto.DailyStudyHours,
                    maxSessionsPerDay = dto.MaxSessionsPerDay,
                    focusSubjects = dto.FocusSubjects ?? new List<string>(),
                    includeStrongSubjects = dto.IncludeStrongSubjects,
                    weekStart = weekStart.ToString("yyyy-MM-dd")
                });

                // 3. Gọi AI
                var aiResponseJson = await _geminiAiService.GenerateStudyPlan(
                    JsonSerializer.Serialize(performanceResult),
                    preferencesJson
                );

                // 4. Parse AI response
                var planDto = JsonSerializer.Deserialize<WeeklyStudyPlanDto>(aiResponseJson, _jsonOptions);
                if (planDto == null)
                {
                    _logger.LogError("Failed to parse AI response: {Response}", aiResponseJson);
                    return ErrorResponse.Build("AI returned invalid plan format", 500);
                }
                if (planDto.DailyPlans == null || !planDto.DailyPlans.Any())
                {
                    // Log raw response for debugging (you already do this for regenerate, add here too)
                    _logger.LogWarning("AI returned empty DailyPlans for user {UserId}. AI response: {Response}", userId, aiResponseJson);
                    // Optionally: you can generate a fallback here or return a clear error to FE
                }

                // 5. Set metadata
                planDto.StudyPlanId = Guid.NewGuid();
                planDto.WeekStart = weekStart;
                planDto.WeekEnd = weekStart.AddDays(6);
                planDto.Status = "Generated";
                planDto.GeneratedAt = StaticOperationStatus.Timezone.Vietnam;
                planDto.CompletionPercentage = 0;

                var serializedPlan = JsonSerializer.Serialize(planDto);

                // 6. Check if a plan for this user + week already exists -> update instead of insert
                var existingPlan = await _unitOfWork.StudyPlan.GetPlanByWeekAsync(userId, weekStart);
                if (existingPlan != null)
                {
                    // Update existing preview to avoid duplicate unique-key violation
                    existingPlan.PlanJson = serializedPlan;
                    existingPlan.Notes = null;
                    existingPlan.Status = "Generated";
                    existingPlan.CompletionPercentage = 0;
                    existingPlan.UpdatedBy = user.FindFirstValue("FullName");
                    existingPlan.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                    _unitOfWork.StudyPlan.Update(existingPlan);
                    await _unitOfWork.SaveAsync();

                    // Ensure DTO StudyPlanId matches stored entity id
                    planDto.StudyPlanId = existingPlan.StudyPlanId;

                    return SuccessResponse.Build(
                        message: "Study plan generated successfully (updated existing preview)",
                        statusCode: 200,
                        result: planDto
                    );
                }

                // 7. Persist preview so FE can save by StudyPlanId later
                var previewEntity = new StudyPlan
                {
                    StudyPlanId = planDto.StudyPlanId,
                    UserId = userId,
                    WeekStart = planDto.WeekStart,
                    WeekEnd = planDto.WeekEnd,
                    PlanJson = JsonSerializer.Serialize(planDto),
                    Notes = null,
                    Status = "Generated",
                    CompletionPercentage = 0,
                    CreatedBy = user.FindFirstValue("FullName"),
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };

                await _unitOfWork.StudyPlan.AddAsync(previewEntity);
                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build(
                    message: "Study plan generated successfully",
                    statusCode: 200,
                    result: planDto
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating study plan for user {UserId}", userId);
                return ErrorResponse.Build($"Failed to generate plan: {ex.Message}", 500);
            }
        }

        // =========================================================
        // 2. SAVE PLAN
        // =========================================================
        public async Task<ResponseDto> SavePlanAsync(Guid planId, SavePlanDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            try
            {
                var entity = await _unitOfWork.StudyPlan.GetAsync(p => p.StudyPlanId == planId && p.UserId == userId);
                if (entity == null)
                    return ErrorResponse.Build("Plan not found", 404);

                // If requested, make this plan active (deactivate others)
                if (dto.SetAsActive)
                {
                    await _unitOfWork.StudyPlan.DeactivateAllPlansAsync(userId);
                    entity.Status = StaticOperationStatus.BaseEntity.Active;
                }
                else
                {
                    entity.Status = StaticOperationStatus.BaseEntity.Inactive;
                }

                // Update entity
                entity.Notes = dto.Notes;
                entity.Status = StaticOperationStatus.BaseEntity.Active;
                entity.UpdatedBy = user.FindFirstValue("FullName");
                entity.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                _unitOfWork.StudyPlan.Update(entity); // ensure update path (repository supports Update)
                await _unitOfWork.SaveAsync();

                // Return the stored plan DTO to caller
                var planDto = JsonSerializer.Deserialize<WeeklyStudyPlanDto>
                    (entity.PlanJson, _jsonOptions) ?? new WeeklyStudyPlanDto();
                planDto.StudyPlanId = entity.StudyPlanId;
                planDto.Status = entity.Status;
                planDto.SavedAt = entity.UpdatedTime ?? entity.CreatedTime;
                planDto.Notes = entity.Notes;

                return SuccessResponse.Build(
                    message: "Study plan saved successfully",
                    statusCode: 200,
                    result: planDto
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving study plan for user {UserId}", userId);
                return ErrorResponse.Build($"Failed to save plan: {ex.Message}", 500);
            }
        }

        // Alternate implementation (if FE sends full plan):
        public async Task<ResponseDto> SavePlanAsync(WeeklyStudyPlanDto planDto, SavePlanDto saveDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            try
            {
                // 1. Deactivate old plans nếu muốn set plan này là active
                if (saveDto.SetAsActive)
                {
                    await _unitOfWork.StudyPlan.DeactivateAllPlansAsync(userId);
                }

                // 2. Tạo entity
                var entity = new StudyPlan
                {
                    StudyPlanId = Guid.NewGuid(),
                    UserId = userId,
                    WeekStart = planDto.WeekStart,
                    WeekEnd = planDto.WeekEnd,
                    PlanJson = JsonSerializer.Serialize(planDto),
                    Notes = saveDto.Notes,
                    Status = StaticOperationStatus.BaseEntity.Active,
                    CompletionPercentage = 0,
                    CreatedBy = user.FindFirstValue("FullName"),
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };

                await _unitOfWork.StudyPlan.AddAsync(entity);
                await _unitOfWork.SaveAsync();

                // 3. Update DTO metadata
                planDto.StudyPlanId = entity.StudyPlanId;
                planDto.Status = entity.Status;
                planDto.SavedAt = entity.UpdatedTime ?? entity.CreatedTime;
                planDto.Notes = saveDto.Notes;

                return SuccessResponse.Build(
                    message: "Study plan saved successfully",
                    statusCode: 201,
                    result: planDto
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving study plan for user {UserId}", userId);
                return ErrorResponse.Build($"Failed to save plan: {ex.Message}", 500);
            }
        }

        // =========================================================
        // 3. GET PLAN BY WEEK
        // =========================================================
        public async Task<ResponseDto> GetPlanByWeekAsync(DateTime weekStart, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            // Normalize date to date-only (drop time component)
            var dateOnly = weekStart.Date;

            // Ensure the DateTime passed to Npgsql has Kind = Utc, because Postgres "timestamp with time zone"
            // requires UTC DateTimes when writing via Npgsql. We don't convert timezone values here — we
            // simply mark this date as UTC to avoid Npgsql ArgumentException.
            var weekStartParam = DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);

            var plan = await _unitOfWork.StudyPlan.GetPlanByWeekAsync(userId, weekStartParam);
            if (plan == null)
                return ErrorResponse.Build("No plan found for this week", 404);

            var planDto = _mapper.Map<WeeklyStudyPlanDto>(plan);
            return SuccessResponse.Build("Plan retrieved successfully", 200, planDto);
        }

        // =========================================================
        // 4. GET ACTIVE PLAN
        // =========================================================
        public async Task<ResponseDto> GetActivePlanAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            var plan = await _unitOfWork.StudyPlan.GetActivePlanAsync(userId);
            if (plan == null)
                return ErrorResponse.Build("No active plan found", 404);

            var planDto = _mapper.Map<WeeklyStudyPlanDto>(plan);
            return SuccessResponse.Build("Active plan retrieved successfully", 200, planDto);
        }

        // =========================================================
        // 5. GET PLAN HISTORY
        // =========================================================
        public async Task<ResponseDto> GetPlanHistoryAsync(ClaimsPrincipal user, int months = 3, int page = 1, int pageSize = 10)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            var (plans, totalCount) = await _unitOfWork.StudyPlan.GetPlanHistoryAsync(userId, months, page, pageSize);
            var historyDtos = plans.Select(p => _mapper.Map<PlanHistoryDto>(p)).ToList();

            return SuccessResponse.Build(
                message: "Plan history retrieved successfully",
                statusCode: 200,
                result: new
                {
                    Data = historyDtos,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                }
            );
        }

        // =========================================================
        // 6. DELETE PLAN
        // =========================================================
        public async Task<ResponseDto> DeletePlanAsync(Guid planId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            var plan = await _unitOfWork.StudyPlan.GetAsync(p => p.StudyPlanId == planId && p.UserId == userId);
            if (plan == null)
                return ErrorResponse.Build("Plan not found", 404);

            // Soft delete
            plan.Status = StaticOperationStatus.BaseEntity.Inactive;
            plan.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
            plan.UpdatedBy = user.FindFirstValue("FullName");

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build("Plan deleted successfully", 200);
        }

        // =========================================================
        // 7. UPDATE SESSION STATUS
        // =========================================================
        public async Task<ResponseDto> UpdateSessionStatusAsync(Guid planId, UpdateSessionDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            var plan = await _unitOfWork.StudyPlan.GetAsync(p => p.StudyPlanId == planId && p.UserId == userId);
            if (plan == null)
                return ErrorResponse.Build("Plan not found", 404);

            // Parse, update, serialize back
            var planDto = JsonSerializer.Deserialize<WeeklyStudyPlanDto>(plan.PlanJson, _jsonOptions);
            if (planDto == null)
                return ErrorResponse.Build("Invalid plan format", 500);

            var dayPlan = planDto.DailyPlans.FirstOrDefault(d => d.DayOfWeek == dto.DayOfWeek);
            if (dayPlan == null)
                return ErrorResponse.Build($"Day {dto.DayOfWeek} not found in plan", 404);

            var session = dayPlan.Sessions.FirstOrDefault(s => s.SessionOrder == dto.SessionOrder);
            if (session == null)
                return ErrorResponse.Build($"Session {dto.SessionOrder} not found", 404);

            // Update session
            session.IsCompleted = dto.IsCompleted;
            session.CompletionNotes = dto.CompletionNotes;
            session.CompletedAt = dto.IsCompleted ? StaticOperationStatus.Timezone.Vietnam : null;

            // Recalculate completion percentage
            var totalSessions = planDto.DailyPlans.Sum(d => d.Sessions.Count);
            var completedSessions = planDto.DailyPlans.Sum(d => d.Sessions.Count(s => s.IsCompleted));
            plan.CompletionPercentage = totalSessions > 0 ? Math.Round((double)completedSessions / totalSessions * 100, 2) : 0;

            // Save back
            plan.PlanJson = JsonSerializer.Serialize(planDto);
            plan.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
            plan.UpdatedBy = user.FindFirstValue("FullName");

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build("Session updated successfully", 200, new { CompletionPercentage = plan.CompletionPercentage });
        }

        // =========================================================
        // 8. REGENERATE PLAN
        // =========================================================
        public async Task<ResponseDto> RegeneratePlanAsync(Guid planId, RegeneratePlanDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build("Unauthorized", 401);

            var planEntity = await _unitOfWork.StudyPlan.GetAsync(p => p.StudyPlanId == planId && p.UserId == userId);
            if (planEntity == null) return ErrorResponse.Build("Plan not found", 404);

            // 1. Parse Plan cũ
            var currentPlan = JsonSerializer.Deserialize<WeeklyStudyPlanDto>(planEntity.PlanJson, _jsonOptions);
            if (currentPlan == null) return ErrorResponse.Build("Invalid stored plan format", 500);

            // 2. Tính toán ngày bắt đầu và số ngày còn lại
            var startDate = dto.StartDate.Date;

            if (startDate < currentPlan.WeekStart.Date || startDate > currentPlan.WeekEnd.Date)
            {
                return ErrorResponse.Build(
                    $"StartDate ({startDate:yyyy-MM-dd}) must be within the plan's week " +
                    $"({currentPlan.WeekStart:yyyy-MM-dd} to {currentPlan.WeekEnd:yyyy-MM-dd}).",
                    400
                );
            }

            // Nếu FE không gửi RemainingDays (null), tự động tính số ngày từ StartDate đến WeekEnd
            var remainingDays = dto.RemainingDays ?? (int)(planEntity.WeekEnd.Date - startDate).TotalDays + 1;
            remainingDays = Math.Clamp(remainingDays, 1, 7);

            try
            {
                // 3. Cập nhật lại kết quả học tập mới nhất
                var performance = await AnalyzeSubjectPerformance(userId, dto.LookbackDays);

                // 4. Fallback Logic: Sử dụng `??` để lấy giá trị mặc định hoặc từ Plan cũ nếu FE không gửi
                var preferencesJson = JsonSerializer.Serialize(new
                {
                    dailyStudyHours = dto.DailyStudyHours ?? 2.0,
                    maxSessionsPerDay = dto.MaxSessionsPerDay ?? 2,
                    focusSubjects = dto.FocusSubjects ?? currentPlan.Performance.WeakSubjects.Select(s => s.Subject).ToList(),
                    includeStrongSubjects = dto.IncludeStrongSubjects ?? false,
                    weekStart = startDate.ToString("yyyy-MM-dd"),
                    remainingDays = remainingDays
                });

                // 5. Gọi AI
                var aiResponseJson = await _geminiAiService.GenerateStudyPlan(JsonSerializer.Serialize(performance), preferencesJson);
                var generated = JsonSerializer.Deserialize<WeeklyStudyPlanDto>(aiResponseJson, _jsonOptions);

                if (generated?.DailyPlans == null || !generated.DailyPlans.Any())
                    return ErrorResponse.Build("AI failed to regenerate plan segment", 500);

                // 6. Merge Logic: Bảo toàn dữ liệu cũ
                var newDaysMap = generated.DailyPlans.ToDictionary(d => d.Date.Date, d => d);

                for (var i = 0; i < currentPlan.DailyPlans.Count; i++)
                {
                    var targetDate = currentPlan.DailyPlans[i].Date.Date;

                    if (newDaysMap.TryGetValue(targetDate, out var newDay))
                    {
                        // Bảo toàn trạng thái IsCompleted của các session trùng lặp
                        foreach (var newSess in newDay.Sessions)
                        {
                            var oldSess = currentPlan.DailyPlans[i].Sessions
                                .FirstOrDefault(s => s.SessionOrder == newSess.SessionOrder);

                            if (oldSess != null && oldSess.IsCompleted)
                            {
                                newSess.IsCompleted = true;
                                newSess.CompletedAt = oldSess.CompletedAt;
                                newSess.CompletionNotes = oldSess.CompletionNotes;
                            }
                        }

                        // Cập nhật ngày mới vào Plan hiện tại
                        currentPlan.DailyPlans[i] = newDay;
                    }
                }

                // 7. Cập nhật tiến độ và lưu DB
                var total = currentPlan.DailyPlans.Sum(d => d.Sessions.Count);
                var completed = currentPlan.DailyPlans.Sum(d => d.Sessions.Count(s => s.IsCompleted));
                planEntity.CompletionPercentage = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0;

                planEntity.PlanJson = JsonSerializer.Serialize(currentPlan);
                planEntity.UpdatedBy = user.FindFirstValue("FullName");
                planEntity.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                _unitOfWork.StudyPlan.Update(planEntity);
                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build("Plan regenerated successfully", 200, currentPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Regeneration error for plan {Id}", planId);
                return ErrorResponse.Build($"Regeneration failed: {ex.Message}", 500);
            }
        }

        // =========================================================
        // 8.5 GET PLAN DETAIL (Dùng cho xem chi tiết từ History)
        // =========================================================
        public async Task<ResponseDto> GetPlanDetailAsync(Guid planId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build("Unauthorized", 401);

            // Lấy entity từ DB
            var plan = await _unitOfWork.StudyPlan.GetAsync(p => p.StudyPlanId == planId && p.UserId == userId);
            if (plan == null) return ErrorResponse.Build("Plan not found", 404);

            // Parse JSON nội dung chi tiết
            var planDto = JsonSerializer.Deserialize<WeeklyStudyPlanDto>(plan.PlanJson, _jsonOptions);
            if (planDto == null) return ErrorResponse.Build("Invalid data format", 500);

            // Sync metadata mới nhất từ DB vào DTO
            planDto.StudyPlanId = plan.StudyPlanId;
            planDto.Status = plan.Status;
            planDto.Notes = plan.Notes;
            planDto.CompletionPercentage = plan.CompletionPercentage;
            planDto.GeneratedAt = plan.CreatedTime ?? DateTime.MinValue;

            return SuccessResponse.Build("Plan detail retrieved", 200, planDto);
        }

        // =========================================================
        // 9. GET SUBJECT PERFORMANCE (Helper exposed as endpoint)
        // =========================================================
        public async Task<ResponseDto> GetSubjectPerformanceAsync(ClaimsPrincipal user, int lookbackDays = 30)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build("Unauthorized", 401);

            var performance = await AnalyzeSubjectPerformance(userId, lookbackDays);
            if (performance == null)
                return ErrorResponse.Build("No exam history found", 404);

            return SuccessResponse.Build("Performance analysis retrieved", 200, performance);
        }


        // =========================================================
        // PRIVATE HELPERS
        // =========================================================
        private async Task<SubjectPerformanceSummaryDto?> AnalyzeSubjectPerformance(string userId, int lookbackDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-lookbackDays);
            var attempts = await _unitOfWork.ExamAttempt.GetAllAsync(a =>
                a.UserId == userId &&
                a.CompletedAt.HasValue &&
                a.CompletedAt >= cutoffDate
            );

            if (!attempts.Any())
                return null;

            // Load ExamPapers to get Subject
            var paperIds = attempts.Select(a => a.ExamPaperId).Distinct();
            var papers = await _unitOfWork.ExamPaper.GetAllAsync(p => paperIds.Contains(p.ExamPaperId));

            // Group by Subject
            var subjectStats = attempts
                .Join(papers, a => a.ExamPaperId, p => p.ExamPaperId, (a, p) => new { Attempt = a, Subject = p.Subject ?? "General" })
                .GroupBy(x => x.Subject)
                .Select(g => new SubjectStatDto
                {
                    Subject = g.Key,
                    AverageScore = Math.Round(g.Average(x => x.Attempt.Score), 2),
                    AttemptCount = g.Count(),
                    CorrectRate = Math.Round(g.Average(x => (double)x.Attempt.CorrectCount / x.Attempt.TotalQuestions * 100), 2),
                    Trend = CalculateTrend(g.OrderBy(x => x.Attempt.CompletedAt).Select(x => x.Attempt.Score).ToList())
                })
                .OrderBy(s => s.AverageScore)
                .ToList();

            // Assign priority (1 = worst, 5 = best)
            for (int i = 0; i < subjectStats.Count; i++)
            {
                subjectStats[i].Priority = Math.Min(i + 1, 5);
            }

            return new SubjectPerformanceSummaryDto
            {
                WeakSubjects = subjectStats.Where(s => s.AverageScore < 7.0).ToList(),
                StrongSubjects = subjectStats.Where(s => s.AverageScore >= 7.0).ToList()
            };
        }

        private static SubjectPerformanceSummaryDto CreateDefaultPerformance(List<string>? focusSubjects)
        {
            var weak = new List<SubjectStatDto>();
            if (focusSubjects != null && focusSubjects.Any())
            {
                int priority = 1;
                foreach (var s in focusSubjects.Distinct())
                {
                    weak.Add(new SubjectStatDto
                    {
                        Subject = s,
                        AverageScore = 0,
                        AttemptCount = 0,
                        CorrectRate = 0,
                        Trend = "Stable",
                        Priority = Math.Min(priority++, 5)
                    });
                }
            }

            return new SubjectPerformanceSummaryDto
            {
                WeakSubjects = weak,
                StrongSubjects = new List<SubjectStatDto>()
            };
        }

        private static string CalculateTrend(List<double> scores)
        {
            if (scores.Count < 2) return "Stable";

            var recent = scores.TakeLast(3).Average();
            var older = scores.Take(scores.Count - 3).DefaultIfEmpty(recent).Average();

            if (recent > older + 0.5) return "Improving";
            if (recent < older - 0.5) return "Declining";
            return "Stable";
        }

        private static DateTime GetNextMonday()
        {
            var today = DateTime.UtcNow.Date;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            return daysUntilMonday == 0 ? today.AddDays(7) : today.AddDays(daysUntilMonday);
        }
    }
}
