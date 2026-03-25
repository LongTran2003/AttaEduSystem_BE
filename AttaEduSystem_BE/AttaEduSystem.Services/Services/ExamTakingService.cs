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
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == submitDto.ExamPaperId);
        if (examPaper == null)
            return ErrorResponse.Build("Exam paper not found", 404);

        var questions = (await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(submitDto.ExamPaperId)).ToList();
        if (!questions.Any())
            return ErrorResponse.Build("This exam has no questions to grade", 400);

        ExamAttempt attempt;
        var isUpdateExistingAttempt = submitDto.ExamAttemptId.HasValue;

        if (isUpdateExistingAttempt)
        {
            attempt = await _unitOfWork.ExamAttempt.GetAttemptWithDetailsAsync(submitDto.ExamAttemptId.Value);
            if (attempt == null)
                return ErrorResponse.Build("Exam attempt not found", 404);

            if (attempt.UserId != userId)
                return ErrorResponse.Build("You are not authorized to submit this exam", 403);

            if (attempt.ExamPaperId != submitDto.ExamPaperId)
                return ErrorResponse.Build("Exam attempt does not match exam paper", 400);

            if (attempt.CompletedAt.HasValue)
                return ErrorResponse.Build("Exam has already been submitted", 400);
        }
        else
        {
            var startedAt = submitDto.StartedAt == default ? StaticOperationStatus.Timezone.Vietnam : submitDto.StartedAt;
            attempt = new ExamAttempt
            {
                ExamAttemptId = Guid.NewGuid(),
                ExamPaperId = submitDto.ExamPaperId,
                UserId = userId,
                StartedAt = startedAt,
                CreatedBy = userId,
                CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                Status = "InProgress"
            };
        }

        var questionMap = questions.ToDictionary(q => q.QuestionId);
        var answerMap = submitDto.Answers
            .GroupBy(a => a.ExamQuestionId)
            .ToDictionary(g => g.Key, g => g.Last());

        int correctCount = 0;
        var attemptDetails = new List<ExamAttemptDetail>();

        foreach (var question in questions)
        {
            answerMap.TryGetValue(question.QuestionId, out var submittedAnswer);
            var userAnswer = submittedAnswer?.UserAnswer?.Trim();
            var isCorrect = !string.IsNullOrWhiteSpace(question.CorrectAnswer) &&
                            !string.IsNullOrWhiteSpace(userAnswer) &&
                            string.Equals(question.CorrectAnswer.Trim(), userAnswer, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
                correctCount++;

            attemptDetails.Add(new ExamAttemptDetail
            {
                ExamAttemptDetailId = Guid.NewGuid(),
                ExamAttemptId = attempt.ExamAttemptId,
                ExamQuestionId = question.QuestionId,
                UserAnswer = userAnswer,
                IsCorrect = isCorrect,
                CreatedBy = userId,
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            });
        }

        double score = questions.Count > 0 ? (double)correctCount / questions.Count * 10 : 0;

        attempt.Score = Math.Round(score, 2);
        attempt.CorrectCount = correctCount;
        attempt.TotalQuestions = questions.Count;
        attempt.CompletedAt = StaticOperationStatus.Timezone.Vietnam;
        attempt.Status = "Submitted";
        attempt.UpdatedBy = userId;
        attempt.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

        if (isUpdateExistingAttempt)
        {
            if (attempt.Details.Any())
                _unitOfWork.ExamAttemptDetail.RemoveRange(attempt.Details);

            await _unitOfWork.ExamAttemptDetail.AddRangeAsync(attemptDetails);
            _unitOfWork.ExamAttempt.Update(attempt);
        }
        else
        {
            attempt.Details = attemptDetails;
            await _unitOfWork.ExamAttempt.AddAsync(attempt);
        }

        await _unitOfWork.SaveAsync();

        var resultDto = new ExamResultDto
        {
            ExamAttemptId = attempt.ExamAttemptId,
            ExamTitle = examPaper.Title,
            Score = attempt.Score,
            CorrectCount = attempt.CorrectCount,
            TotalQuestions = attempt.TotalQuestions,
            CompletedAt = attempt.CompletedAt.Value,
            Details = attemptDetails.Select(d => new ExamResultDetailDto
            {
                ExamQuestionId = d.ExamQuestionId,
                UserAnswer = d.UserAnswer ?? string.Empty,
                IsCorrect = d.IsCorrect,
                CorrectAnswer = questionMap.TryGetValue(d.ExamQuestionId, out var q) ? q.CorrectAnswer ?? string.Empty : string.Empty
            }).ToList(),
            LearningRecommendations = BuildLearningRecommendations(questions, attemptDetails, examPaper.Subject)
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
        var questions = attempt.Details
            .Where(d => d.ExamQuestion != null)
            .Select(d => d.ExamQuestion)
            .DistinctBy(q => q.QuestionId)
            .ToList();
        resultDto.LearningRecommendations = BuildLearningRecommendations(questions, attempt.Details.ToList(), attempt.ExamPaper?.Subject);

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
        resultDto.LearningRecommendations = BuildLearningRecommendations(questions.ToList(), attempt.Details.ToList(), attempt.ExamPaper?.Subject);

        return SuccessResponse.Build("Exam auto-submitted successfully", 200, resultDto);
    }

    private static List<string> BuildLearningRecommendations(
        List<ExamQuestion> questions,
        List<ExamAttemptDetail> attemptDetails,
        string? subject)
    {
        var recommendations = new List<string>();
        var wrongDetails = attemptDetails.Where(d => !d.IsCorrect).ToList();

        if (!wrongDetails.Any())
        {
            recommendations.Add("Bạn làm rất tốt. Hãy luyện thêm đề khó hơn để duy trì phong độ.");
            return recommendations;
        }

        var wrongRatio = (double)wrongDetails.Count / Math.Max(1, questions.Count);
        recommendations.Add(wrongRatio >= 0.5
            ? "Bạn nên ôn lại nền tảng lý thuyết trước khi làm thêm đề mới."
            : "Bạn đã nắm cơ bản tốt, nên tập trung sửa các lỗi sai trọng điểm.");

        var wrongQuestionMap = wrongDetails
            .Select(d => questions.FirstOrDefault(q => q.QuestionId == d.ExamQuestionId))
            .Where(q => q != null)
            .ToList();

        var dominantType = wrongQuestionMap
            .Where(q => !string.IsNullOrWhiteSpace(q!.QuestionType))
            .GroupBy(q => q!.QuestionType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(dominantType))
            recommendations.Add($"Bạn sai nhiều ở dạng câu hỏi {dominantType}. Hãy luyện riêng nhóm dạng này theo bộ câu hỏi nhỏ.");

        var wrongIndexes = wrongQuestionMap
            .Select(q => q!.OrderIndex)
            .OrderBy(i => i)
            .Take(5)
            .ToList();

        if (wrongIndexes.Any())
            recommendations.Add($"Ưu tiên xem lại các câu số: {string.Join(", ", wrongIndexes)} và tự giải lại không nhìn đáp án.");

        if (!string.IsNullOrWhiteSpace(subject))
            recommendations.Add($"Gợi ý lộ trình {subject}: học lại lý thuyết cốt lõi, làm 20-30 câu theo chuyên đề sai, rồi làm lại 1 đề tổng hợp.");

        return recommendations.Take(5).ToList();
    }
}
