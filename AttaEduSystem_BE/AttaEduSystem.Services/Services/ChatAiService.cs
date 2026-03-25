using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class ChatAiService : IChatAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _geminiApiUrl;
        private readonly string? _openAiApiKey;
        private readonly string _openAiModel;
        private readonly ILogger<ChatAiService> _logger;

        public ChatAiService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var geminiApiKey = configuration["Gemini:ApiKey"];
            if (!string.IsNullOrWhiteSpace(geminiApiKey))
            {
                _geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={geminiApiKey}";
            }

            _openAiApiKey = configuration["OpenAI:ApiKey"];
            _openAiModel = configuration["ChatAI:OpenAIModel"] ?? "gpt-4o-mini";
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
            var geminiReply = await TryCallGeminiApi(payload);
            if (!string.IsNullOrWhiteSpace(geminiReply))
                return geminiReply;

            var openAiReply = await TryCallOpenAiAsync(userMessage, conversationHistory);
            if (!string.IsNullOrWhiteSpace(openAiReply))
                return openAiReply;

            throw new InvalidOperationException("No AI provider available");
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

        private async Task<string?> TryCallGeminiApi(object payload)
        {
            if (string.IsNullOrWhiteSpace(_geminiApiUrl))
                return null;

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_geminiApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);
                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
                    candidates.ValueKind != JsonValueKind.Array ||
                    candidates.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Gemini response has no candidates: {Body}", responseString);
                    return null;
                }

                var firstCandidate = candidates[0];
                if (!firstCandidate.TryGetProperty("content", out var candidateContent))
                    return null;
                if (!candidateContent.TryGetProperty("parts", out var parts) ||
                    parts.ValueKind != JsonValueKind.Array ||
                    parts.GetArrayLength() == 0)
                    return null;
                if (!parts[0].TryGetProperty("text", out var textNode))
                    return null;

                var text = textNode.GetString();

                return text ?? "Không có phản hồi từ AI";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini Chat API");
                return null;
            }
        }

        private async Task<string?> TryCallOpenAiAsync(string userMessage, List<(string Role, string Content)>? conversationHistory)
        {
            if (string.IsNullOrWhiteSpace(_openAiApiKey))
                return null;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);

                var messages = new List<object>
                {
                    new { role = "system", content = GetSystemPrompt() }
                };

                if (conversationHistory != null)
                {
                    foreach (var (role, content) in conversationHistory.TakeLast(10))
                    {
                        messages.Add(new
                        {
                            role = role.ToLower() == "user" ? "user" : "assistant",
                            content
                        });
                    }
                }

                messages.Add(new { role = "user", content = userMessage });

                var payload = new
                {
                    model = _openAiModel,
                    messages,
                    temperature = 0.3
                };

                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API Error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
                    choices.ValueKind != JsonValueKind.Array ||
                    choices.GetArrayLength() == 0)
                    return null;
                if (!choices[0].TryGetProperty("message", out var message))
                    return null;
                if (!message.TryGetProperty("content", out var contentNode))
                    return null;

                return contentNode.GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI Chat API");
                return null;
            }
        }
    }
}
