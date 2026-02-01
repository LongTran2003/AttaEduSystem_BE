using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Openai;
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
    public class ExamGeneratingService : IExamGeneratingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeminiAiService _geminiAiService;
        private readonly IMapper _mapper;
        private readonly ILogger<ExamGeneratingService> _logger;
        private readonly IUsageTrackerService _usageTracker;

        public ExamGeneratingService(
            IUnitOfWork unitOfWork, 
            IGeminiAiService geminiAiService, 
            ILogger<ExamGeneratingService> logger, 
            IMapper mapper, IUsageTrackerService usageTracker)
        {
            _unitOfWork = unitOfWork;
            _geminiAiService = geminiAiService;
            _logger = logger;
            _mapper = mapper;
            _usageTracker = usageTracker;
        }

        public async Task<ResponseDto> GenerateSimilarExam(Guid originalExamPaperId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                // ========== CHECK QUOTA ==========
                if (!await _usageTracker.TryConsumeAsync(user, UsageType.GenerateExam, 1))
                {
                    return ErrorResponse.Build(
                        message: "Your exam generation quota has been reached. Please upgrade your subscription to continue.",
                        statusCode: 402);
                }
                // ==================================

                // 1. Lấy đề thi gốc từ DB
                var originalExam = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == originalExamPaperId);
                if (originalExam == null || string.IsNullOrEmpty(originalExam.ScannedText))
                {
                    return ErrorResponse.Build("Original exam content not found", 404);
                }

                // 2. Gọi Gemini tạo đề mới
                string newExamJson;
                try
                {
                    newExamJson = await _geminiAiService.GenerateSimilarExam(originalExam.ScannedText);
                }
                catch (Exception ex)
                {
                    return ErrorResponse.Build("AI Generation Failed: " + ex.Message, 500);
                }

                // 3. Lưu đề mới vào DB
                var generatedExam = new GeneratedExamPaper
                {
                    OriginalExamPaperId = originalExamPaperId,
                    GeneratedContentJson = newExamJson,
                    AiModelUsed = "gemini-2.5-flash",
                    CreatedBy = userId,
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                    Status = "Draft"
                };

                await _unitOfWork.GeneratedExamPaper.AddAsync(generatedExam);
                await _unitOfWork.SaveAsync();

                var responseDto = _mapper.Map<GenerateExamResponseDto>(generatedExam);

                return SuccessResponse.Build("New exam generated successfully", 201, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating exam");
                return ErrorResponse.Build("Internal Server Error", 500);
            }
        }

        public async Task<ResponseDto> UpdateStatus(Guid generatedExamId, string status, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

            var exam = await _unitOfWork.GeneratedExamPaper.GetAsync(x => x.GeneratedExamPaperId == generatedExamId);
            if (exam == null) return ErrorResponse.Build("Generated exam not found", 404);

            // Check quyền sở hữu
            if (exam.CreatedBy != userId) return ErrorResponse.Build("You do not have permission to update this exam", 403);

            exam.Status = status;
            exam.UpdatedBy = userId;
            exam.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.GeneratedExamPaper.Update(exam);
            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build("Generated exam status updated successfully", 200);
        }
    }
}
