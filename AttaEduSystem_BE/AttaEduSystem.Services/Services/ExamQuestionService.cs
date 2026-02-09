using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class ExamQuestionService : IExamQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExamQuestionService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // =========================================================
        // GET QUESTION BY ID
        // =========================================================
        public async Task<ResponseDto> GetQuestionById(Guid questionId)
        {
            try
            {
                var question = await _unitOfWork.ExamQuestion.GetByIdWithOptionsAsync(questionId);
                if (question == null)
                    return ErrorResponse.Build("Question not found", 404);

                var dto = _mapper.Map<ExamQuestionDto>(question);
                return SuccessResponse.Build("Question retrieved successfully", 200, dto);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve question: {ex.Message}", 500);
            }
        }

        // =========================================================
        // UPDATE QUESTION
        // =========================================================
        public async Task<ResponseDto> UpdateQuestion(Guid questionId, UpdateExamQuestionDto dto, ClaimsPrincipal user)
        {
            try
            {
                // 1. Check user authentication
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                // 2. Get question with options
                var question = await _unitOfWork.ExamQuestion.GetByIdWithOptionsAsync(questionId);
                if (question == null)
                    return ErrorResponse.Build("Question not found", 404);

                // 3. Validate ownership
                var (isValid, errorMessage, statusCode, _) = await ValidateExamOwnershipAsync(question.ExamPaperId, userId);
                if (!isValid)
                    return ErrorResponse.Build(errorMessage!, statusCode);

                // 4. Update question properties
                question.Content = dto.Content;
                question.QuestionIdLabel = dto.QuestionIdLabel;
                question.Points = dto.Points;
                question.QuestionType = dto.QuestionType;
                question.CorrectAnswer = dto.CorrectAnswer;
                question.DifficultyLevel = dto.DifficultyLevel;
                question.UpdatedBy = userId;
                question.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                // 5. Handle Options update (remove old, add new)
                if (dto.Options != null)
                {
                    if (question.Options.Any())
                    {
                        _unitOfWork.QuestionOption.RemoveRange(question.Options);
                    }

                    var newOptions = dto.Options.Select(o => new QuestionOption
                    {
                        OptionId = Guid.NewGuid(),
                        QuestionId = question.QuestionId,
                        Label = o.OptionLabel,
                        Content = o.OptionContent
                    }).ToList();

                    await _unitOfWork.QuestionOption.AddRangeAsync(newOptions);
                }

                // 6. Save changes
                _unitOfWork.ExamQuestion.Update(question);
                await _unitOfWork.SaveAsync();

                // 7. Return updated question
                var updatedQuestion = await _unitOfWork.ExamQuestion.GetByIdWithOptionsAsync(questionId);
                var responseDto = _mapper.Map<ExamQuestionDto>(updatedQuestion);

                return SuccessResponse.Build("Question updated successfully", 200, responseDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to update question: {ex.Message}", 500);
            }
        }

        // =========================================================
        // DELETE QUESTION
        // =========================================================
        public async Task<ResponseDto> DeleteQuestion(Guid questionId, ClaimsPrincipal user)
        {
            try
            {
                // 1. Check user authentication
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                // 2. Get question
                var question = await _unitOfWork.ExamQuestion.GetByIdWithOptionsAsync(questionId);
                if (question == null)
                    return ErrorResponse.Build("Question not found", 404);

                // 3. Validate ownership
                var examPaperId = question.ExamPaperId;
                var (isValid, errorMessage, statusCode, _) = await ValidateExamOwnershipAsync(examPaperId, userId);
                if (!isValid)
                    return ErrorResponse.Build(errorMessage!, statusCode);

                // 4. Remove options first (if any)
                if (question.Options.Any())
                {
                    _unitOfWork.QuestionOption.RemoveRange(question.Options);
                }

                // 5. Remove question
                _unitOfWork.ExamQuestion.Remove(question);

                // 6. Reorder remaining questions
                var remainingQuestions = await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(examPaperId);
                var questionsToReorder = remainingQuestions
                    .Where(q => q.QuestionId != questionId)
                    .OrderBy(q => q.OrderIndex)
                    .ToList();

                for (int i = 0; i < questionsToReorder.Count; i++)
                {
                    questionsToReorder[i].OrderIndex = i + 1;
                    questionsToReorder[i].UpdatedBy = userId;
                    questionsToReorder[i].UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                }

                if (questionsToReorder.Any())
                {
                    _unitOfWork.ExamQuestion.UpdateRange(questionsToReorder);
                }

                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build("Question deleted successfully", 200);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to delete question: {ex.Message}", 500);
            }
        }

        // =========================================================
        // ADD QUESTION TO EXAM PAPER
        // =========================================================
        public async Task<ResponseDto> AddQuestionToExamPaper(Guid examPaperId, AddExamQuestionDto dto, ClaimsPrincipal user)
        {
            try
            {
                // 1. Check user authentication
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                // 2. Validate ownership
                var (isValid, errorMessage, statusCode, _) = await ValidateExamOwnershipAsync(examPaperId, userId);
                if (!isValid)
                    return ErrorResponse.Build(errorMessage!, statusCode);

                // 3. Get current max OrderIndex
                var existingQuestions = await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(examPaperId);
                var maxOrderIndex = existingQuestions.Any() ? existingQuestions.Max(q => q.OrderIndex) : 0;

                // 4. Create new question
                var question = new ExamQuestion
                {
                    QuestionId = Guid.NewGuid(),
                    ExamPaperId = examPaperId,
                    Content = dto.Content,
                    QuestionIdLabel = dto.QuestionIdLabel ?? $"Câu {maxOrderIndex + 1}",
                    Points = dto.Points,
                    OrderIndex = maxOrderIndex + 1,
                    QuestionType = dto.QuestionType,
                    CorrectAnswer = dto.CorrectAnswer,
                    DifficultyLevel = dto.DifficultyLevel,
                    CreatedBy = user.FindFirstValue("FullName") ?? userId,
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };

                await _unitOfWork.ExamQuestion.AddAsync(question);

                // 5. Add options if provided
                if (dto.Options != null && dto.Options.Any())
                {
                    var options = dto.Options.Select(o => new QuestionOption
                    {
                        OptionId = Guid.NewGuid(),
                        QuestionId = question.QuestionId,
                        Label = o.OptionLabel,
                        Content = o.OptionContent
                    }).ToList();

                    await _unitOfWork.QuestionOption.AddRangeAsync(options);
                }

                await _unitOfWork.SaveAsync();

                // 6. Return created question with options
                var createdQuestion = await _unitOfWork.ExamQuestion.GetByIdWithOptionsAsync(question.QuestionId);
                var responseDto = _mapper.Map<ExamQuestionDto>(createdQuestion);

                return SuccessResponse.Build("Question added successfully", 201, responseDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to add question: {ex.Message}", 500);
            }
        }

        // =========================================================
        // REORDER QUESTIONS
        // =========================================================
        public async Task<ResponseDto> ReorderQuestions(Guid examPaperId, ReorderQuestionsDto dto, ClaimsPrincipal user)
        {
            try
            {
                // 1. Check user authentication
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                // 2. Validate ownership
                var (isValid, errorMessage, statusCode, _) = await ValidateExamOwnershipAsync(examPaperId, userId);
                if (!isValid)
                    return ErrorResponse.Build(errorMessage!, statusCode);

                // 3. Get all questions of this exam
                var questions = await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(examPaperId);
                if (!questions.Any())
                    return ErrorResponse.Build("No questions found for this exam paper", 404);

                // 4. Validate all question IDs belong to this exam
                var questionIds = questions.Select(q => q.QuestionId).ToHashSet();
                foreach (var orderItem in dto.QuestionOrders)
                {
                    if (!questionIds.Contains(orderItem.QuestionId))
                        return ErrorResponse.Build($"Question {orderItem.QuestionId} does not belong to this exam paper", 400);
                }

                // 5. Update order indexes
                foreach (var orderItem in dto.QuestionOrders)
                {
                    var question = questions.First(q => q.QuestionId == orderItem.QuestionId);
                    question.OrderIndex = orderItem.OrderIndex;
                    question.UpdatedBy = userId;
                    question.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                }

                _unitOfWork.ExamQuestion.UpdateRange(questions);
                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build("Questions reordered successfully", 200);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to reorder questions: {ex.Message}", 500);
            }
        }

        // =========================================================
        // GET QUESTIONS BY EXAM PAPER
        // =========================================================
        public async Task<ResponseDto> GetQuestionsByExamPaper(Guid examPaperId)
        {
            try
            {
                var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
                if (examPaper == null)
                    return ErrorResponse.Build("Exam paper not found", 404);

                var questions = await _unitOfWork.ExamQuestion.GetByExamPaperIdAsync(examPaperId);
                var dtos = _mapper.Map<List<ExamQuestionDto>>(questions);

                return SuccessResponse.Build("Questions retrieved successfully", 200, dtos);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve questions: {ex.Message}", 500);
            }
        }



        // =========================================================
        // HELPER: Validate Exam Ownership
        // =========================================================
        private async Task<(bool IsValid, string? ErrorMessage, int StatusCode, ExamPaper? ExamPaper)>
            ValidateExamOwnershipAsync(Guid examPaperId, string userId)
        {
            var examPaper = await _unitOfWork.ExamPaper.GetByIdWithUserAsync(examPaperId);

            if (examPaper == null)
                return (false, "Exam paper not found", 404, null);

            // Check ownership: Creator.Id hoặc CreatedBy
            if (examPaper.Creator?.Id != userId && examPaper.CreatedBy != userId)
                return (false, "You do not have permission to modify this exam paper", 403, null);

            return (true, null, 200, examPaper);
        }
    }
}
