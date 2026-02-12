using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.SharedExam;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class ShareService : IShareService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShareService> _logger;
        private readonly IMapper _mapper;

        public ShareService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<ShareService> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ResponseDto> CreateShareLink(CreateShareLinkDto dto, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build("User not found", 401);

                bool sourceExists = dto.SourceType.ToLower() switch
                {
                    "exampaper" => await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == dto.SourceId) != null,
                    "generatedexam" => await _unitOfWork.GeneratedExamPaper.GetAsync
                            (g => g.GeneratedExamPaperId == dto.SourceId) != null,
                    _ => false
                };

                if (!sourceExists)
                    return ErrorResponse.Build($"{dto.SourceType} with ID {dto.SourceId} not found", 404);

                var token = await GenerateUniqueTokenAsync();

                // Use AutoMapper for entity creation
                var sharedExam = _mapper.Map<SharedExam>(dto);
                sharedExam.SharedExamId = Guid.NewGuid();
                sharedExam.ShareToken = token;
                sharedExam.ExpiresAt = dto.ExpiresInDays.HasValue ? DateTime.UtcNow.AddDays(dto.ExpiresInDays.Value) : null;
                sharedExam.Password = !string.IsNullOrEmpty(dto.Password)
                    ? BCrypt.Net.BCrypt.HashPassword(dto.Password)
                    : null;
                sharedExam.IsActive = true;
                sharedExam.ViewCount = 0;
                sharedExam.CreatedBy = userId;
                sharedExam.CreatedTime = DateTime.UtcNow;

                await _unitOfWork.SharedExam.AddAsync(sharedExam);
                await _unitOfWork.SaveAsync();

                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://yourapp.com";

                // Use AutoMapper for response
                var response = _mapper.Map<ShareLinkResponseDto>(sharedExam);
                response.ShareUrl = $"{baseUrl}/shared/{token}";
                response.HasPassword = !string.IsNullOrEmpty(dto.Password);
                response.CreatedAt = DateTime.UtcNow;

                return SuccessResponse.Build("Share link created successfully", 201, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share link");
                return ErrorResponse.Build($"Failed to create share link: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> GetSharedExam(string token, string? password = null)
        {
            try
            {
                var sharedExam = await _unitOfWork.SharedExam.GetByTokenAsync(token);

                if (sharedExam == null)
                {
                    return ErrorResponse.Build("Share link not found", 404);
                }

                // Check if active
                if (!sharedExam.IsActive)
                {
                    return ErrorResponse.Build("This share link has been deactivated", 410);
                }

                // Check expiration
                if (sharedExam.ExpiresAt.HasValue && sharedExam.ExpiresAt < DateTime.UtcNow)
                {
                    return ErrorResponse.Build("This share link has expired", 410);
                }

                // Check max views
                if (sharedExam.MaxViews.HasValue && sharedExam.ViewCount >= sharedExam.MaxViews)
                {
                    return ErrorResponse.Build("This share link has reached maximum views", 410);
                }

                // Check password
                if (!string.IsNullOrEmpty(sharedExam.Password))
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        return ErrorResponse.Build("Password required", 401);
                    }

                    if (!BCrypt.Net.BCrypt.Verify(password, sharedExam.Password))
                    {
                        return ErrorResponse.Build("Invalid password", 401);
                    }
                }

                // Get exam data
                SharedExamViewDto? viewDto = sharedExam.SourceType.ToLower() switch
                {
                    "exampaper" => await GetExamPaperViewAsync(sharedExam),
                    "generatedexam" => await GetGeneratedExamViewAsync(sharedExam),
                    _ => null
                };

                if (viewDto == null)
                {
                    return ErrorResponse.Build("Exam data not found", 404);
                }

                // Increment view count
                sharedExam.ViewCount++;
                _unitOfWork.SharedExam.Update(sharedExam);
                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build("Shared exam retrieved successfully", 200, viewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared exam");
                return ErrorResponse.Build($"Failed to get shared exam: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> GetMyShareLinks(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return ErrorResponse.Build("User not found", 401);
                }

                var shareLinks = await _unitOfWork.SharedExam.GetByUserIdAsync(userId);

                var result = new List<MyShareLinkDto>();

                foreach (var link in shareLinks)
                {
                    var examTitle = await GetExamTitleAsync(link.SourceType, link.SourceId);

                    result.Add(new MyShareLinkDto
                    {
                        SharedExamId = link.SharedExamId,
                        ShareToken = link.ShareToken,
                        SourceType = link.SourceType,
                        ExamTitle = examTitle,
                        ExpiresAt = link.ExpiresAt,
                        HasPassword = !string.IsNullOrEmpty(link.Password),
                        ViewCount = link.ViewCount,
                        IsActive = link.IsActive,
                        CreatedAt = link.CreatedTime ?? DateTime.UtcNow
                    });
                }

                return SuccessResponse.Build("Share links retrieved successfully", 200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting share links");
                return ErrorResponse.Build($"Failed to get share links: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> DeactivateShareLink(Guid sharedExamId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return ErrorResponse.Build("User not found", 401);
                }

                var sharedExam = await _unitOfWork.SharedExam.GetAsync(s => s.SharedExamId == sharedExamId);

                if (sharedExam == null)
                {
                    return ErrorResponse.Build("Share link not found", 404);
                }

                if (sharedExam.CreatedBy != userId)
                {
                    return ErrorResponse.Build("You can only deactivate your own share links", 403);
                }

                sharedExam.IsActive = false;
                sharedExam.UpdatedBy = userId;
                sharedExam.UpdatedTime = DateTime.UtcNow;

                _unitOfWork.SharedExam.Update(sharedExam);
                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build("Share link deactivated successfully", 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating share link");
                return ErrorResponse.Build($"Failed to deactivate share link: {ex.Message}", 500);
            }
        }

        // =========================================================
        // HELPER METHODS
        // =========================================================
        private async Task<string> GenerateUniqueTokenAsync()
        {
            string token;
            do
            {
                token = GenerateToken(8);
            } while (await _unitOfWork.SharedExam.TokenExistsAsync(token));

            return token;
        }

        private static string GenerateToken(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }

        private async Task<SharedExamViewDto?> GetExamPaperViewAsync(SharedExam sharedExam)
        {
            var exam = await _unitOfWork.ExamPaper.GetAsync(
                e => e.ExamPaperId == sharedExam.SourceId,
                includeProperties: "Questions,Questions.Options,Creator");

            if (exam == null) return null;

            return new SharedExamViewDto
            {
                Title = exam.Title,
                Subject = exam.Subject,
                Description = exam.Description,
                SharedBy = exam.Creator?.FullName ?? sharedExam.CreatedBy ?? "Unknown",
                ExpiresAt = sharedExam.ExpiresAt,
                ViewCount = sharedExam.ViewCount + 1,
                Questions = exam.Questions?
                    .OrderBy(q => q.OrderIndex)
                    .Select(q => new SharedQuestionDto
                    {
                        QuestionNumber = q.OrderIndex,
                        Content = q.Content,
                        QuestionType = q.QuestionType ?? "MultipleChoice",
                        Options = q.Options?.OrderBy(o => o.Label).Select(o => $"{o.Label}. {o.Content}").ToList() ?? new(),
                        CorrectAnswer = sharedExam.IncludeAnswers ? q.CorrectAnswer : null
                    }).ToList() ?? new(),
                CanExportPdf = true,
                CanExportWord = true
            };
        }

        private async Task<SharedExamViewDto?> GetGeneratedExamViewAsync(SharedExam sharedExam)
        {
            var generated = await _unitOfWork.GeneratedExamPaper.GetAsync(
                g => g.GeneratedExamPaperId == sharedExam.SourceId,
                includeProperties: "OriginalExamPaper");

            if (generated == null) return null;

            // Parse JSON
            var parsed = JsonSerializer.Deserialize<Models.DTOs.GeminiAi.ExamStructureResponse>(
                generated.GeneratedContentJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var title = parsed?.ExamInfo?.GetValueOrDefault("title")
                ?? generated.OriginalExamPaper?.Title
                ?? "Generated Exam";

            return new SharedExamViewDto
            {
                Title = title,
                Subject = parsed?.ExamInfo?.GetValueOrDefault("suggested_subject"),
                Description = $"AI Generated Exam",
                SharedBy = sharedExam.CreatedBy ?? "Unknown",
                ExpiresAt = sharedExam.ExpiresAt,
                ViewCount = sharedExam.ViewCount + 1,
                Questions = parsed?.Questions?
                    .Select((q, i) => new SharedQuestionDto
                    {
                        QuestionNumber = i + 1,
                        Content = q.Content,
                        QuestionType = q.Type ?? "MultipleChoice",
                        Options = q.Options?.ToList() ?? new(),
                        CorrectAnswer = null // Generated exams don't have answers
                    }).ToList() ?? new(),
                CanExportPdf = true,
                CanExportWord = true
            };
        }

        private async Task<string> GetExamTitleAsync(string sourceType, Guid sourceId)
        {
            if (sourceType.ToLower() == "exampaper")
            {
                var exam = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == sourceId);
                return exam?.Title ?? "Unknown";
            }
            else
            {
                var generated = await _unitOfWork.GeneratedExamPaper.GetAsync(g => g.GeneratedExamPaperId == sourceId);
                return generated?.OriginalExamPaper?.Title ?? "Generated Exam";
            }
        }
    }
}
