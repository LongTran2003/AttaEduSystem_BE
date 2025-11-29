using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace AttaEduSystem.Services.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiService> _logger;
        private readonly IMapper _mapper;

        public OpenAiService(
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<OpenAiService> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ResponseDto> GenerateExamAsync(GenerateExamRequestDto requestDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

            var original = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == requestDto.OriginalExamPaperId);
            if (original == null)
                return ErrorResponse.Build("Original exam not found", 404);

            var prompt = BuildPrompt(original, requestDto);
            var aiModel = requestDto.AiModel ?? _configuration["ExamGeneration:DefaultModel"] ?? "gpt-4o-mini";

            string generatedContent;
            try
            {
                generatedContent = await CallOpenAiAsync(prompt, aiModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI generation failed");
                return ErrorResponse.Build("Failed to generate exam via AI", 500);
            }

            var generatedExam = new GeneratedExamPaper
            {
                GeneratedExamPaperId = Guid.NewGuid(),
                OriginalExamPaperId = original.ExamPaperId,
                GeneratedContent = generatedContent,
                AiModelUsed = aiModel,
                PromptSnapshot = prompt,
                CreatedBy = userId,
                CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                Status = StaticOperationStatus.ExamPaper.Ready
            };

            await _unitOfWork.GeneratedExamPaper.AddAsync(generatedExam);
            await _unitOfWork.SaveAsync();

            var dto = new GenerateExamResponseDto
            {
                GeneratedExamId = generatedExam.GeneratedExamPaperId,
                GeneratedContent = generatedContent,
                AiModelUsed = aiModel,
                PromptSnapshot = prompt,
                GeneratedAt = generatedExam.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam
            };

            return SuccessResponse.Build("Generated exam successfully", 201, dto);
        }

        public async Task<ResponseDto> GetGeneratedExamAsync(Guid generatedExamId, ClaimsPrincipal user)
        {
            var generatedExam = await _unitOfWork.GeneratedExamPaper.GetAsync(g => g.GeneratedExamPaperId == generatedExamId);
            if (generatedExam == null)
                return ErrorResponse.Build("Generated exam not found", 404);

            var dto = _mapper.Map<GenerateExamResponseDto>(generatedExam);
            return SuccessResponse.Build("Generated exam retrieved", 200, dto);
        }

        public async Task<ResponseDto> GetGeneratedExamsByOriginalAsync(Guid examPaperId, ClaimsPrincipal user)
        {
            var items = await _unitOfWork.GeneratedExamPaper.GetByOriginalExamAsync(examPaperId);
            var dtoList = _mapper.Map<IEnumerable<GenerateExamResponseDto>>(items);

            return SuccessResponse.Build("Generated exams retrieved", 200, dtoList);
        }



        private string BuildPrompt(ExamPaper original, GenerateExamRequestDto request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a professional exam designer.");
            sb.AppendLine("Generate a new exam mirroring the structure, tone and point distribution of the original.");
            sb.AppendLine($"Original exam format JSON: {original.ExamFormat ?? "{}"}");
            sb.AppendLine("Original OCR text:");
            sb.AppendLine(original.ScannedText ?? string.Empty);

            if (request.NumberOfQuestions.HasValue)
                sb.AppendLine($"Target number of questions: {request.NumberOfQuestions.Value}");

            if (!string.IsNullOrWhiteSpace(request.CustomInstructions))
            {
                sb.AppendLine("Additional instructions:");
                sb.AppendLine(request.CustomInstructions);
            }

            sb.AppendLine("Return the exam in structured markdown with sections and point values.");
            return sb.ToString();
        }

        private async Task<string> CallOpenAiAsync(string prompt, string model)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("OpenAI API key missing");

            var httpClient = _httpClientFactory.CreateClient("OpenAI");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var body = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful exam creation assistant." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
            return json?.choices?.FirstOrDefault()?.message?.content
                   ?? throw new Exception("OpenAI returned empty content.");
        }

        private sealed class OpenAiChatResponse
        {
            public List<Choice>? choices { get; set; }

            public sealed class Choice
            {
                public ChatMessage? message { get; set; }
            }

            public sealed class ChatMessage
            {
                public string? role { get; set; }
                public string? content { get; set; }
            }
        }
    }
}
