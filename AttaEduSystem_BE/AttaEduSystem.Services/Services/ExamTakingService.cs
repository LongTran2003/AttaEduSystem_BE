using System.Security.Claims;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamResult;
using AttaEduSystem.Models.DTOs.ExamTaking;
using AttaEduSystem.Models.DTOs.SubmitExam;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;

namespace AttaEduSystem.Services.Services;

public class ExamTakingService : IExamTakingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExamTakingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseDto> SubmitExam(SubmitExamDto submitDto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

            // 1. Lấy đề thi và đáp án đúng
            // Lưu ý: Cần Include ExamQuestions để lấy CorrectAnswer
            var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == submitDto.ExamPaperId);
            if (examPaper == null) return ErrorResponse.Build("Exam paper not found", 404);
            
            // Giả sử bạn có phương thức lấy câu hỏi theo ExamId
            // var questions = await _unitOfWork.ExamQuestion.GetAllAsync(q => q.ExamPaperId == submitDto.ExamPaperId); 
            var questions = await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(submitDto.ExamPaperId); 

            if (questions == null || !questions.Any())
                return ErrorResponse.Build("This exam has no questions to grade", 400);

            // 2. Tính điểm
            int correctCount = 0;
            var attemptDetails = new List<ExamAttemptDetail>();

            foreach (var answer in submitDto.Answers)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == answer.ExamQuestionId);
                bool isCorrect = false;

                if (question != null && !string.IsNullOrEmpty(question.CorrectAnswer))
                {
                    // So sánh không phân biệt hoa thường
                    if (string.Equals(question.CorrectAnswer.Trim(), answer.UserAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrect = true;
                        correctCount++;
                    }
                }

                attemptDetails.Add(new ExamAttemptDetail
                {
                    ExamAttemptDetailId = Guid.NewGuid(),
                    ExamQuestionId = answer.ExamQuestionId,
                    UserAnswer = answer.UserAnswer,
                    IsCorrect = isCorrect
                });
            }

            // Tính điểm trên thang 10
            double score = questions.Count() > 0 ? (double)correctCount / questions.Count() * 10 : 0;

            // 3. Lưu kết quả
            var attempt = new ExamAttempt
            {
                ExamAttemptId = Guid.NewGuid(),
                ExamPaperId = submitDto.ExamPaperId,
                UserId = userId,
                Score = Math.Round(score, 2),
                CorrectCount = correctCount,
                TotalQuestions = questions.Count(),
                StartedAt = submitDto.StartedAt,
                CompletedAt = DateTime.UtcNow,
                Details = attemptDetails
            };

            await _unitOfWork.ExamAttempt.AddAsync(attempt);
            await _unitOfWork.SaveAsync();

            // 4. Trả về kết quả ngay lập tức
            var resultDto = new ExamResultDto
            {
                ExamAttemptId = attempt.ExamAttemptId,
                Score = attempt.Score,
                CorrectCount = attempt.CorrectCount,
                TotalQuestions = attempt.TotalQuestions,
                CompletedAt = attempt.CompletedAt.Value,
                Details = attemptDetails.Select(d => new ExamResultDetailDto 
                {
                    ExamQuestionId = d.ExamQuestionId,
                    UserAnswer = d.UserAnswer,
                    IsCorrect = d.IsCorrect,
                    CorrectAnswer = questions.FirstOrDefault(q => q.QuestionId == d.ExamQuestionId)?.CorrectAnswer
                }).ToList()
            };

            return SuccessResponse.Build("Exam submitted successfully", 200, resultDto);
    }

    public async Task<ResponseDto> GetExamHistory(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        // 1. Gọi Repo lấy dữ liệu (đã Include sẵn ExamPaper)
        var attempts = await _unitOfWork.ExamAttempt.GetHistoryByUserIdAsync(userId);

        if (attempts == null || !attempts.Any())
        {
            return SuccessResponse.Build("No exam history found", 200, new List<ExamHistoryDto>());
        }

        // 2. Dùng AutoMapper thay vì Select thủ công
        var historyDtos = _mapper.Map<List<ExamHistoryDto>>(attempts);

        return SuccessResponse.Build("Exam history retrieved successfully", 200, historyDtos);
    }

    public async Task<ResponseDto> GetExamResult(Guid attemptId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        // 1. Gọi Repo lấy chi tiết (đã Include ExamPaper, Details, ExamQuestion)
        var attempt = await _unitOfWork.ExamAttempt.GetAttemptWithDetailsAsync(attemptId);

        if (attempt == null)
        {
            return ErrorResponse.Build("Exam attempt not found", 404);
        }

        // 2. Check quyền
        if (attempt.UserId != userId) 
        {
            return ErrorResponse.Build("You are not authorized to view this result", 403);
        }

        // 3. Dùng AutoMapper (Nó sẽ tự map cả Attempt lẫn List Details bên trong)
        var resultDto = _mapper.Map<ExamResultDto>(attempt);

        return SuccessResponse.Build("Exam result retrieved successfully", 200, resultDto);
    }

    // =========================================================
    // AUTO-SUBMIT EXAM (When time's up)
    // =========================================================
    public async Task<ResponseDto> AutoSubmitExam(Guid examAttemptId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        // 1. Lấy ExamAttempt
        var attempt = await _unitOfWork.ExamAttempt.GetAttemptWithDetailsAsync(examAttemptId);
        if (attempt == null)
            return ErrorResponse.Build("Exam attempt not found", 404);

        if (attempt.UserId != userId)
            return ErrorResponse.Build("You are not authorized to submit this exam", 403);

        if (attempt.CompletedAt.HasValue)
            return ErrorResponse.Build("Exam has already been submitted", 400);

        // 2. Lấy tất cả câu hỏi của đề
        var questions = await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(attempt.ExamPaperId);
        if (questions == null || !questions.Any())
            return ErrorResponse.Build("No questions found", 400);

        // 3. Tính điểm dựa trên câu đã trả lời (Details đã có sẵn từ quá trình làm bài)
        int correctCount = 0;
        foreach (var detail in attempt.Details)
        {
            var question = questions.FirstOrDefault(q => q.QuestionId == detail.ExamQuestionId);
            if (question != null && !string.IsNullOrEmpty(question.CorrectAnswer))
            {
                if (string.Equals(question.CorrectAnswer.Trim(), detail.UserAnswer?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    detail.IsCorrect = true;
                    correctCount++;
                }
            }
        }

        // 4. Cập nhật điểm
        double score = questions.Count > 0 ? (double)correctCount / questions.Count * 10 : 0;

        attempt.Score = Math.Round(score, 2);
        attempt.CorrectCount = correctCount;
        attempt.TotalQuestions = questions.Count;
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Status = "AutoSubmitted";

        _unitOfWork.ExamAttempt.Update(attempt);
        await _unitOfWork.SaveAsync();

        // 5. Trả về kết quả
        var resultDto = _mapper.Map<ExamResultDto>(attempt);

        return SuccessResponse.Build("Exam auto-submitted successfully", 200, resultDto);
    }
}