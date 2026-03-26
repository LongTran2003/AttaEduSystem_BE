using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class ChatAiService : IChatAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly ILogger<ChatAiService> _logger;

        public ChatAiService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var apiKey = ResolveGeminiApiKey(configuration);
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
        }

        public async Task<string> GetChatResponseAsync(string userMessage, List<(string Role, string Content)>? conversationHistory = null)
        {
            // Build conversation context
            var contents = new List<object>();

            // System instruction
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = GetSystemPrompt() } }
            });
            contents.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Understood! I'm ready to assist with educational topics." } }
            });

            // Add conversation history (nếu có)
            if (conversationHistory != null)
            {
                foreach (var (role, content) in conversationHistory)
                {
                    contents.Add(new
                    {
                        role = role.ToLower() == "user" ? "user" : "model",
                        parts = new[] { new { text = content } }
                    });
                }
            }

            // Add current user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var payload = new { contents };

            return await CallGeminiApi(payload);
        }

        private string GetSystemPrompt()
        {
            return @"Bạn là trợ lý AI giáo dục thông minh của hệ thống AttaEdu. 
Nhiệm vụ của bạn:
1. Hỗ trợ học sinh/giáo viên về các vấn đề học tập, giải bài tập
2. Giải thích các khái niệm một cách dễ hiểu
3. Công thức toán học viết dạng LaTeX (đặt giữa dấu $)
4. Trả lời bằng tiếng Việt trừ khi được yêu cầu khác
5. Từ chối trả lời các nội dung không liên quan đến giáo dục hoặc không phù hợp";
        }

        private async Task<string> CallGeminiApi(object payload)
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                    throw new Exception($"Gemini API Error ({(int)response.StatusCode}): {errorBody}");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Không có phản hồi từ AI";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini Chat API");
                throw;
            }
        }

        private static string ResolveGeminiApiKey(IConfiguration configuration)
        {
            var apiKey = new[]
            {
                configuration["Gemini:ApiKey"],
                configuration["Gemini__ApiKey"],
                configuration["Gemini_ApiKey"],
                configuration["Gemini_Apikey"]
            }.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API Key not configured");
            }

            return apiKey;
        }
    }
}
