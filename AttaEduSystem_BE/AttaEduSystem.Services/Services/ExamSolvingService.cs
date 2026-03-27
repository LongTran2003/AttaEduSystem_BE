using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.GeminiAi;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class ExamSolvingService : IExamSolvingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeminiAiService _geminiAiService;
        private readonly IMapper _mapper;
        private readonly ILogger<ExamSolvingService> _logger;
        private readonly IUsageTrackerService _usageTracker;

        public ExamSolvingService(
            IUnitOfWork unitOfWork, 
            IGeminiAiService geminiAiService, 
            ILogger<ExamSolvingService> logger, 
            IMapper mapper, 
            IUsageTrackerService usageTracker)
        {
            _unitOfWork = unitOfWork;
            _geminiAiService = geminiAiService;
            _logger = logger;
            _mapper = mapper;
            _usageTracker = usageTracker;
        }

        public async Task<ResponseDto> SolveExamPaper(Guid examPaperId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var roleValues = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isTeacherOrAdmin = roleValues.Any(r =>
                    string.Equals(r, StaticUserRoles.Teacher, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r, StaticUserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r, "Teacher", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));

                if (!isTeacherOrAdmin && !await _usageTracker.TryConsumeAsync(user, UsageType.Solve, 1))
                {
                    return ErrorResponse.Build(
                        message: "Your solve quota has been reached. Please upgrade your subscription to continue.",
                        statusCode: 402);
                }

                // 1. Lấy đề thi từ DB
                var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
                if (examPaper == null) return ErrorResponse.Build("Exam paper not found", 404);

                // Ưu tiên lấy từ ScannedText (JSON gốc) vì nó đầy đủ nhất để AI hiểu ngữ cảnh
                if (string.IsNullOrEmpty(examPaper.ScannedText))
                {
                    return ErrorResponse.Build("Exam content not available. Please scan first.", 400);
                }

                // 2. Gọi Gemini giải đề
                string solutionJson;
                try
                {
                    solutionJson = await _geminiAiService.SolveExam(examPaper.ScannedText);
                }
                catch (Exception ex)
                {
                    return ErrorResponse.Build("AI Solving Failed: " + ex.Message, 500);
                }

                // 3. Upsert lời giải (update nếu đã có, tạo mới nếu chưa có)
                var existing = await _unitOfWork.ExamSolution.GetAsync(
                    s => s.ExamPaperId == examPaperId && s.Status != "Deleted");

                ExamSolution solution;
                if (existing != null)
                {
                    // Re-solve: ghi đè nội dung cũ
                    existing.SolutionContentJson = solutionJson;
                    existing.UpdatedBy = userId;
                    existing.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                    existing.Status = "Saved";
                    _unitOfWork.ExamSolution.Update(existing);
                    solution = existing;
                }
                else
                {
                    solution = new ExamSolution
                    {
                        ExamPaperId = examPaperId,
                        SolutionContentJson = solutionJson,
                        CreatedBy = userId,
                        CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                        Status = "Saved"
                    };
                    await _unitOfWork.ExamSolution.AddAsync(solution);
                }

                await _unitOfWork.SaveAsync();

                var responseDto = _mapper.Map<ExamSolutionResponseDto>(solution);
                return SuccessResponse.Build(
                    existing != null ? "Exam re-solved successfully" : "Exam solved successfully",
                    201, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error solving exam");
                return ErrorResponse.Build("Internal Server Error", 500);
            }
        }

        public async Task<ResponseDto> GetSolutionByExamId(Guid examPaperId)
        {
            // Logic lấy solution từ DB...
            var solution = await _unitOfWork.ExamSolution.GetAsync(s => s.ExamPaperId == examPaperId);
            if (solution == null) return ErrorResponse.Build("Solution not found", 404);

            var responseDto = _mapper.Map<ExamSolutionResponseDto>(solution);

            return SuccessResponse.Build("Retrieved solution", 200, responseDto);
        }

        public async Task<ResponseDto> UpdateStatus(Guid solutionId, string status, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

            var solution = await _unitOfWork.ExamSolution.GetAsync(s => s.ExamSolutionId == solutionId);
            if (solution == null) return ErrorResponse.Build("Solution not found", 404);

            if (solution.CreatedBy != userId) return ErrorResponse.Build("You do not have permission to update this solution", 403);

            solution.Status = status;
            solution.UpdatedBy = userId;
            solution.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.ExamSolution.Update(solution);
            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build("Solution status updated successfully", 200);
        }

        // =========================================================
        // NEW: Paging / GetById / GetByUser
        // =========================================================
        public async Task<ResponseDto> GetAllSolutions(
            int pageNumber = 1,
            int pageSize = 10,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null)
        {
            try
            {
                var (items, totalCount) = await _unitOfWork.ExamSolution.GetSolutionsAsync(
                    pageNumber, pageSize, filterOn, filterQuery, sortBy, includeProperties: "ExamPaper");

                if (items == null || !items.Any())
                {
                    var emptyResult = new
                    {
                        Data = Enumerable.Empty<ExamSolutionResponseDto>(),
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0,
                        TotalPages = 0,
                        HasPreviousPage = false,
                        HasNextPage = false
                    };

                    return SuccessResponse.Build("No solutions found", 200, emptyResult);
                }

                var dtos = _mapper.Map<IEnumerable<ExamSolutionResponseDto>>(items);
                var payload = new
                {
                    Data = dtos,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber * pageSize < totalCount
                };

                return SuccessResponse.Build("Solutions retrieved successfully", 200, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solutions");
                return ErrorResponse.Build("Failed to retrieve solutions", 500);
            }
        }

        public async Task<ResponseDto> GetSolutionById(Guid solutionId)
        {
            try
            {
                var solution = await _unitOfWork.ExamSolution.GetByIdWithExamAsync(solutionId);
                if (solution == null) return ErrorResponse.Build("Solution not found", 404);

                var dto = _mapper.Map<ExamSolutionResponseDto>(solution);
                return SuccessResponse.Build("Solution retrieved successfully", 200, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solution by ID");
                return ErrorResponse.Build("Failed to retrieve solution", 500);
            }
        }

        public async Task<ResponseDto> GetSolutionsByUser(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var solutions = await _unitOfWork.ExamSolution.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<IEnumerable<ExamSolutionResponseDto>>(solutions.OrderByDescending(s => s.CreatedTime));

                return SuccessResponse.Build("User solutions retrieved successfully", 200, dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user solutions");
                return ErrorResponse.Build("Failed to retrieve user solutions", 500);
            }
        }
        public async Task<ResponseDto> UpdateSolutionContentAsync(
            Guid solutionId, string solutionContentJson, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var solution = await _unitOfWork.ExamSolution.GetAsync(s => s.ExamSolutionId == solutionId);
                if (solution == null)
                    return ErrorResponse.Build("Solution not found", 404);

                // Chỉ owner hoặc admin mới được sửa
                var isAdmin = user.IsInRole("Admin");
                if (solution.CreatedBy != userId && !isAdmin)
                    return ErrorResponse.Build("You do not have permission to edit this solution", 403);

                solution.SolutionContentJson = solutionContentJson;
                solution.UpdatedBy = userId;
                solution.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                _unitOfWork.ExamSolution.Update(solution);
                await _unitOfWork.SaveAsync();

                var responseDto = _mapper.Map<ExamSolutionResponseDto>(solution);
                return SuccessResponse.Build("Solution updated successfully", 200, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating solution content {SolutionId}", solutionId);
                return ErrorResponse.Build($"Failed to update solution: {ex.Message}", 500);
            }
        }
    }
}
