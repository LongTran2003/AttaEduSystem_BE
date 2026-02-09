using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Models.DTOs.ExamShuffle;
using AttaEduSystem.Models.DTOs.QuestionBank;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using System.Security.Claims;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class ExamShuffleService : IExamShuffleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private static readonly string[] VariantCodes = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
        private static readonly string[] OptionLabels = { "A", "B", "C", "D", "E", "F" };

        public ExamShuffleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // =========================================================
        // SHUFFLE EXAM
        // =========================================================
        public async Task<ResponseDto> ShuffleExam(Guid examPaperId, ShuffleExamRequestDto dto, ClaimsPrincipal user)
        {
            try
            {
                // 1. Check user authentication
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                // 2. Validate ownership
                var (isValid, errorMessage, statusCode, examPaper) = await ValidateExamOwnershipAsync(examPaperId, userId);
                if (!isValid)
                    return ErrorResponse.Build(errorMessage!, statusCode);

                // 3. Get questions with options
                var questions = await _unitOfWork.ExamQuestion.GetByExamPaperIdWithOptionsAsync(examPaperId);
                if (!questions.Any())
                    return ErrorResponse.Build("No questions found for this exam paper", 404);

                // 4. Generate shuffled variants
                var variants = new List<ShuffledExamVariantDto>();
                var random = new Random();

                for (int i = 0; i < dto.NumberOfVariants; i++)
                {
                    var variant = GenerateShuffledVariant(
                        questions,
                        examPaper!,
                        VariantCodes[i],
                        dto.ShuffleQuestions,
                        dto.ShuffleOptions,
                        random);

                    variants.Add(variant);

                    // 5. Save to GeneratedExamPaper
                    var generatedExam = new GeneratedExamPaper
                    {
                        GeneratedExamPaperId = variant.VariantId,
                        OriginalExamPaperId = examPaperId,
                        GeneratedContentJson = JsonSerializer.Serialize(variant),
                        AiModelUsed = "Shuffle",
                        PromptSnapshot = $"ShuffleQuestions={dto.ShuffleQuestions}, ShuffleOptions={dto.ShuffleOptions}",
                        CreatedBy = user.FindFirstValue("FullName"),
                        CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                        Status = "Ready"
                    };

                    await _unitOfWork.GeneratedExamPaper.AddAsync(generatedExam);
                }

                await _unitOfWork.SaveAsync();

                // 6. Build response
                var response = new ShuffleExamResponseDto
                {
                    OriginalExamPaperId = examPaperId,
                    OriginalExamTitle = examPaper!.Title ?? "Untitled",
                    TotalVariants = dto.NumberOfVariants,
                    Variants = variants
                };

                return SuccessResponse.Build("Exam shuffled successfully", 201, response);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to shuffle exam: {ex.Message}", 500);
            }
        }

        // =========================================================
        // GET SHUFFLED VARIANTS
        // =========================================================
        public async Task<ResponseDto> GetShuffledVariants(Guid examPaperId, ClaimsPrincipal user)
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

                // 3. Get all shuffled variants
                var generatedExams = await _unitOfWork.GeneratedExamPaper.GetByOriginalExamAsync(examPaperId);
                var shuffledVariants = generatedExams
                    .Where(g => g.AiModelUsed == "Shuffle")
                    .ToList();

                if (!shuffledVariants.Any())
                    return SuccessResponse.Build("No shuffled variants found", 200, new List<object>());

                // 4. Parse JSON and return
                var variants = shuffledVariants.Select(v =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<ShuffledExamVariantDto>(v.GeneratedContentJson);
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(v => v != null).ToList();

                return SuccessResponse.Build("Shuffled variants retrieved successfully", 200, variants);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve shuffled variants: {ex.Message}", 500);
            }
        }

        // =========================================================
        // SEARCH QUESTION BANK
        // =========================================================
        public async Task<ResponseDto> SearchQuestionBank(QuestionBankFilterDto filterDto, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get questions from repository
                var (questions, totalCount) = await _unitOfWork.ExamQuestion.SearchQuestionsAsync(
                    filterDto.SearchTerm,
                    filterDto.QuestionType,
                    filterDto.DifficultyLevel,
                    filterDto.Subject,
                    userId,
                    filterDto.Scope,
                    filterDto.PageNumber,
                    filterDto.PageSize);

                if (!questions.Any())
                {
                    return SuccessResponse.Build("No questions found", 200, new QuestionBankResponseDto
                    {
                        Data = new List<QuestionBankItemDto>(),
                        CurrentPage = filterDto.PageNumber,
                        PageSize = filterDto.PageSize,
                        TotalCount = 0,
                        TotalPages = 0
                    });
                }

                // Map to DTOs
                var questionDtos = questions.Select(q => new QuestionBankItemDto
                {
                    QuestionId = q.QuestionId,
                    ExamPaperId = q.ExamPaperId,
                    ExamPaperTitle = q.ExamPaper?.Title ?? "Unknown",
                    Subject = q.ExamPaper?.Subject,
                    Content = q.Content,
                    QuestionIdLabel = q.QuestionIdLabel,
                    QuestionType = q.QuestionType,
                    DifficultyLevel = q.DifficultyLevel,
                    Points = q.Points,
                    CorrectAnswer = q.CorrectAnswer,
                    Options = q.Options.Select(o => new QuestionBankOptionDto
                    {
                        OptionId = o.OptionId,
                        OptionLabel = o.Label,
                        OptionContent = o.Content
                    }).ToList(),
                    CreatedBy = q.CreatedBy ?? "Unknown",
                    CreatedTime = q.CreatedTime ?? DateTime.UtcNow
                }).ToList();

                var response = new QuestionBankResponseDto
                {
                    Data = questionDtos,
                    CurrentPage = filterDto.PageNumber,
                    PageSize = filterDto.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filterDto.PageSize)
                };

                return SuccessResponse.Build("Questions retrieved successfully", 200, response);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to search questions: {ex.Message}", 500);
            }
        }



        // =========================================================
        // HELPER: Generate Shuffled Variant
        // =========================================================
        private ShuffledExamVariantDto GenerateShuffledVariant(
            List<ExamQuestion> questions,
            ExamPaper examPaper,
            string variantCode,
            bool shuffleQuestions,
            bool shuffleOptions,
            Random random)
        {
            // Clone and shuffle questions
            var shuffledQuestions = questions.ToList();
            if (shuffleQuestions)
            {
                shuffledQuestions = shuffledQuestions.OrderBy(_ => random.Next()).ToList();
            }

            var variant = new ShuffledExamVariantDto
            {
                VariantId = Guid.NewGuid(),
                VariantCode = variantCode,
                OriginalExamPaperId = examPaper.ExamPaperId,
                OriginalExamTitle = examPaper.Title ?? "Untitled",
                CreatedAt = StaticOperationStatus.Timezone.Vietnam,
                Questions = new List<ShuffledQuestionDto>()
            };

            for (int i = 0; i < shuffledQuestions.Count; i++)
            {
                var originalQuestion = shuffledQuestions[i];
                var shuffledQuestion = new ShuffledQuestionDto
                {
                    OriginalQuestionId = originalQuestion.QuestionId,
                    NewOrderIndex = i + 1,
                    QuestionIdLabel = $"Câu {i + 1}",
                    Content = originalQuestion.Content,
                    QuestionType = originalQuestion.QuestionType,
                    Points = originalQuestion.Points,
                    Options = new List<ShuffledOptionDto>()
                };

                // Shuffle options if MultipleChoice
                if (originalQuestion.QuestionType == "MultipleChoice" && originalQuestion.Options.Any())
                {
                    var options = originalQuestion.Options.ToList();
                    var originalCorrectLabel = originalQuestion.CorrectAnswer;

                    if (shuffleOptions)
                    {
                        options = options.OrderBy(_ => random.Next()).ToList();
                    }

                    string? newCorrectAnswer = null;

                    for (int j = 0; j < options.Count; j++)
                    {
                        var newLabel = OptionLabels[j];
                        var originalLabel = options[j].Label;

                        // Track new correct answer
                        if (originalLabel == originalCorrectLabel)
                        {
                            newCorrectAnswer = newLabel;
                        }

                        shuffledQuestion.Options.Add(new ShuffledOptionDto
                        {
                            OriginalLabel = originalLabel,
                            NewLabel = newLabel,
                            Content = options[j].Content
                        });
                    }

                    shuffledQuestion.CorrectAnswer = newCorrectAnswer;
                }
                else
                {
                    shuffledQuestion.CorrectAnswer = originalQuestion.CorrectAnswer;
                }

                variant.Questions.Add(shuffledQuestion);
            }

            return variant;
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

            if (examPaper.Creator?.Id != userId && examPaper.CreatedBy != userId)
                return (false, "You do not have permission to modify this exam paper", 403, null);

            return (true, null, 200, examPaper);
        }

    }
}
