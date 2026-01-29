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

                // ========== CHECK QUOTA ==========
                if (!await _usageTracker.TryConsumeAsync(user, UsageType.Solve, 1))
                {
                    return ErrorResponse.Build(
                        message: "Your solve quota has been reached. Please upgrade your subscription to continue.",
                        statusCode: 402);
                }
                // ==================================

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

                // 3. Lưu lời giải vào DB
                var solution = new ExamSolution
                {
                    ExamPaperId = examPaperId,
                    SolutionContentJson = solutionJson,
                    CreatedBy = userId,
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };

                await _unitOfWork.ExamSolution.AddAsync(solution); 
                await _unitOfWork.SaveAsync();

                var responseDto = _mapper.Map<ExamSolutionResponseDto>(solution);

                return SuccessResponse.Build("Exam solved successfully", 201, responseDto);
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
    }
}
