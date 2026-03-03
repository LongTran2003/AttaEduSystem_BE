using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    /// <summary>
    /// Service thống nhất để phân tích đề thi với multi-tier fallback strategy
    /// Tier 1: Gemini (Free) -> Tier 2: Azure OpenAI (Stable) -> Tier 3: OpenAI (Backup)
    /// </summary>
    public class AiAnalysisService : IAiAnalysisService
    {
        private readonly IGeminiAiService _geminiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiAnalysisService> _logger;

        public AiAnalysisService(
            IGeminiAiService geminiService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AiAnalysisService> logger)
        {
            _geminiService = geminiService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> AnalyzeExamStructureWithFallback(string base64Image, string mimeType)
        {
            // Tier 1: Try Gemini first (cheapest/free)
            try
            {
                _logger.LogInformation("Tier 1: Attempting exam analysis with Gemini AI");
                var result = await _geminiService.AnalyzeExamStructure(base64Image, mimeType);
                _logger.LogInformation("✅ Tier 1: Gemini AI analysis completed successfully");
                return result;
            }
            catch (Exception geminiEx)
            {
                _logger.LogWarning(geminiEx, "❌ Tier 1: Gemini AI failed: {ErrorMessage}", geminiEx.Message);

                // Tier 2: Try Azure OpenAI (most stable)
                try
                {
                    _logger.LogInformation("Tier 2: Attempting exam analysis with Azure OpenAI");
                    var result = await AnalyzeWithAzureOpenAi(base64Image, mimeType);
                    _logger.LogInformation("✅ Tier 2: Azure OpenAI analysis completed successfully");
                    return result;
                }
                catch (Exception azureEx)
                {
                    _logger.LogWarning(azureEx, "❌ Tier 2: Azure OpenAI failed: {ErrorMessage}", azureEx.Message);

                    // Tier 3: Fallback to standard OpenAI Vision (last resort)
                    try
                    {
                        _logger.LogInformation("Tier 3: Attempting exam analysis with OpenAI Vision (last resort)");
                        var result = await AnalyzeWithOpenAiVision(base64Image, mimeType);
                        _logger.LogInformation("✅ Tier 3: OpenAI Vision analysis completed successfully");
                        return result;
                    }
                    catch (Exception openAiEx)
                    {
                        _logger.LogError(openAiEx, "❌ Tier 3: All AI services exhausted");

                        throw new Exception(
                            $"All AI analysis services failed. " +
                            $"Gemini: {geminiEx.Message}. " +
                            $"Azure OpenAI: {azureEx.Message}. " +
                            $"OpenAI: {openAiEx.Message}",
                            openAiEx);
                    }
                }
            }
        }

        #region Azure OpenAI Implementation

        private async Task<string> AnalyzeWithAzureOpenAi(string base64Image, string mimeType)
        {
            var azureEndpoint = _configuration["AzureOpenAI:Endpoint"];
            var azureApiKey = _configuration["AzureOpenAI:ApiKey"];
            var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";

            if (string.IsNullOrEmpty(azureEndpoint) || string.IsNullOrEmpty(azureApiKey))
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing. Please check appsettings.json");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("api-key", azureApiKey);
            httpClient.Timeout = TimeSpan.FromMinutes(2); // Azure OpenAI might be slower

            var prompt = BuildAnalysisPrompt();

            var payload = new
            {
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:{mimeType};base64,{base64Image}",
                                    detail = "high" // High detail for better OCR accuracy
                                }
                            }
                        }
                    }
                },
                max_tokens = 4096,
                temperature = 0.1, // Low temperature for consistent structured output
                response_format = new { type = "json_object" }
            };

            // Azure OpenAI uses different endpoint structure
            var apiVersion = "2024-02-15-preview"; // Latest stable version with vision support
            var url = $"{azureEndpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

            try
            {
                var response = await httpClient.PostAsJsonAsync(url, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Azure OpenAI API error ({response.StatusCode}): {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<AzureOpenAiResponse>();
                var content = result?.choices?.FirstOrDefault()?.message?.content;

                if (string.IsNullOrEmpty(content))
                {
                    throw new Exception("Azure OpenAI returned empty response");
                }

                // Validate JSON structure
                ValidateJsonStructure(content);

                _logger.LogInformation("Azure OpenAI usage: {PromptTokens} prompt + {CompletionTokens} completion = {TotalTokens} total tokens",
                    result?.usage?.prompt_tokens,
                    result?.usage?.completion_tokens,
                    result?.usage?.total_tokens);

                return content;
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error calling Azure OpenAI: {httpEx.Message}", httpEx);
            }
            catch (JsonException jsonEx)
            {
                throw new Exception($"Invalid JSON response from Azure OpenAI: {jsonEx.Message}", jsonEx);
            }
            catch (TaskCanceledException timeoutEx)
            {
                throw new Exception($"Azure OpenAI request timeout: {timeoutEx.Message}", timeoutEx);
            }
        }

        #endregion

        private async Task<string> AnalyzeWithOpenAiVision(string base64Image, string mimeType)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is missing in configuration");
            }

            var httpClient = _httpClientFactory.CreateClient("OpenAI");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var prompt = @"
                Bạn là một AI chuyên gia số hóa đề thi.
                
                NHIỆM VỤ: 
                Phân tích hình ảnh và trích xuất các câu hỏi thành JSON.

                QUY TẮC QUAN TRỌNG (ĐỂ LỌC NHIỄU):
                1. BỎ QUA hoàn toàn phần Header (Tên trường, Sở GD, Họ tên học sinh, Lớp, Mã đề, Lời dặn dò).
                2. BẮT ĐẦU trích xuất từ câu hỏi đầu tiên (thường bắt đầu bằng 'Câu 1', 'Question 1', '1.', 'Bài 1').
                3. Nếu gặp các tiêu đề phần lớn (VD: 'I. TRẮC NGHIỆM'), hãy bỏ qua hoặc gộp vào nội dung câu đầu tiên của phần đó.

                PHÂN LOẠI (Type):
                - 'MultipleChoice': Nếu câu hỏi có các đáp án lựa chọn (A, B, C, D...).
                - 'Essay': Nếu câu hỏi tự luận, điền từ, hoặc không có đáp án trắc nghiệm.

                YÊU CẦU OUTPUT JSON (Schema):
                {
                    ""exam_info"": { 
                        ""title"": ""Trích xuất tiêu đề đề thi (VD: KIỂM TRA 1 TIẾT)"", 
                        ""suggested_subject"": ""Môn học dự đoán""
                    },
                    ""questions"": [
                        {
                            ""id"": ""Câu 1"",
                            ""type"": ""MultipleChoice"" hoặc ""Essay"",
                            ""content"": ""Nội dung câu hỏi. Giữ nguyên LaTeX ($) cho công thức toán."",
                            ""options"": [""A. ..."", ""B. ...""] (Nếu là Essay thì để mảng rỗng [] hoặc null),
                            ""points"": 0.25 (Dự đoán điểm số, mặc định 0.25 cho trắc nghiệm, 1.0 cho tự luận)
                        }
                    ]
                }
                Chỉ trả về JSON thuần, không Markdown.";

            var payload = new
            {
                model = "gpt-4o",  // Model hỗ trợ vision (gpt-4o hoặc gpt-4-turbo)
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:{mimeType};base64,{base64Image}",
                                    detail = "high" // High detail for better OCR
                                }
                            }
                        }
                    }
                },
                max_tokens = 4096,
                temperature = 0.1, // Low temperature for consistent output
                response_format = new { type = "json_object" }
            };

            try
            {
                var response = await httpClient.PostAsJsonAsync("chat/completions", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"OpenAI API error ({response.StatusCode}): {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<OpenAiVisionResponse>();
                var content = result?.choices?.FirstOrDefault()?.message?.content;

                if (string.IsNullOrEmpty(content))
                {
                    throw new Exception("OpenAI returned empty response");
                }

                // Validate JSON structure
                using var jsonDoc = JsonDocument.Parse(content);
                if (!jsonDoc.RootElement.TryGetProperty("questions", out _))
                {
                    throw new Exception("OpenAI response missing required 'questions' property");
                }

                return content;
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error calling OpenAI: {httpEx.Message}", httpEx);
            }
            catch (JsonException jsonEx)
            {
                throw new Exception($"Invalid JSON response from OpenAI: {jsonEx.Message}", jsonEx);
            }
        }

        private static string BuildAnalysisPrompt()
        {
            return @"
                Bạn là một AI chuyên gia số hóa đề thi.
                
                NHIỆM VỤ: 
                Phân tích hình ảnh và trích xuất các câu hỏi thành JSON.

                QUY TẮC QUAN TRỌNG (ĐỂ LỌC NHIỄU):
                1. BỎ QUA hoàn toàn phần Header (Tên trường, Sở GD, Họ tên học sinh, Lớp, Mã đề, Lời dặn dò).
                2. BẮT ĐẦU trích xuất từ câu hỏi đầu tiên (thường bắt đầu bằng 'Câu 1', 'Question 1', '1.', 'Bài 1').
                3. Nếu gặp các tiêu đề phần lớn (VD: 'I. TRẮC NGHIỆM'), hãy bỏ qua hoặc gộp vào nội dung câu đầu tiên của phần đó.

                PHÂN LOẠI (Type):
                - 'MultipleChoice': Nếu câu hỏi có các đáp án lựa chọn (A, B, C, D...).
                - 'Essay': Nếu câu hỏi tự luận, điền từ, hoặc không có đáp án trắc nghiệm.

                YÊU CẦU OUTPUT JSON (Schema):
                {
                    ""exam_info"": { 
                        ""title"": ""Trích xuất tiêu đề đề thi (VD: KIỂM TRA 1 TIẾT)"", 
                        ""suggested_subject"": ""Môn học dự đoán""
                    },
                    ""questions"": [
                        {
                            ""id"": ""Câu 1"",
                            ""type"": ""MultipleChoice"" hoặc ""Essay"",
                            ""content"": ""Nội dung câu hỏi. Giữ nguyên LaTeX ($) cho công thức toán."",
                            ""options"": [""A. ..."", ""B. ...""] (Nếu là Essay thì để mảng rỗng [] hoặc null),
                            ""points"": 0.25 (Dự đoán điểm số, mặc định 0.25 cho trắc nghiệm, 1.0 cho tự luận)
                        }
                    ]
                }
                Chỉ trả về JSON thuần, không Markdown.";
        }

        private static void ValidateJsonStructure(string jsonContent)
        {
            using var jsonDoc = JsonDocument.Parse(jsonContent);

            if (!jsonDoc.RootElement.TryGetProperty("questions", out _))
            {
                throw new Exception("AI response missing required 'questions' property");
            }

            if (!jsonDoc.RootElement.TryGetProperty("exam_info", out _))
            {
                throw new Exception("AI response missing required 'exam_info' property");
            }
        }

        #region OpenAI Response DTOs

        private sealed class AzureOpenAiResponse
        {
            public List<AzureChoice>? choices { get; set; }
            public AzureUsage? usage { get; set; }
        }

        private sealed class AzureChoice
        {
            public int index { get; set; }
            public AzureMessage? message { get; set; }
            public string? finish_reason { get; set; }
        }

        private sealed class AzureMessage
        {
            public string? role { get; set; }
            public string? content { get; set; }
        }

        private sealed class AzureUsage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        private sealed class OpenAiVisionResponse
        {
            public List<Choice>? choices { get; set; }
            public Usage? usage { get; set; }
        }

        private sealed class Choice
        {
            public int index { get; set; }
            public Message? message { get; set; }
            public string? finish_reason { get; set; }
        }

        private sealed class Message
        {
            public string? role { get; set; }
            public string? content { get; set; }
        }

        private sealed class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        #endregion
    }
}
