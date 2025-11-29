using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class GeminiAiService : IGeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key missing");
            _apiUrl =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
        }

        // =========================================================
        // 1. ANALYZE (PHÂN TÍCH ĐỀ TỪ ẢNH)
        // =========================================================
        public async Task<string> AnalyzeExamStructure(string base64Image, string mimeType)
        {
            var prompt = @"
                Bạn là một AI chuyên số hóa đề thi.
                NHIỆM VỤ: Phân tích hình ảnh đề thi và trích xuất cấu trúc JSON.
                
                YÊU CẦU OUTPUT JSON (Schema):
                {
                    ""exam_info"": { ""title"": ""string"", ""time_limit"": ""string"", ""total_points"": ""string"" },
                    ""questions"": [
                        {
                            ""id"": ""Câu 1"",
                            ""content"": ""Nội dung câu hỏi. Công thức toán giữ nguyên LaTeX giữa dấu $."",
                            ""options"": [""A..."", ""B...""] (nếu trắc nghiệm, để null nếu tự luận),
                            ""points"": 1.0
                        }
                    ]
                }
                Chỉ trả về JSON hợp lệ.";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new { inline_data = new { mime_type = mimeType, data = base64Image } }
                        }
                    }
                },
                // Ép Gemini trả về JSON Mode
                generationConfig = new { response_mime_type = "application/json" }
            };

            return await CallGeminiApi(payload);
        }

        // =========================================================
        // 2. SOLVE (GIẢI ĐỀ TỪ JSON)
        // =========================================================
        public async Task<string> SolveExam(string examContentJson)
        {
            var prompt = $@"
                Bạn là giáo viên giỏi. Hãy giải chi tiết đề thi được cung cấp dưới dạng JSON sau đây.
                
                INPUT JSON:
                {examContentJson}

                YÊU CẦU:
                1. Giải từng câu một theo thứ tự.
                2. Trình bày lời giải từng bước (Step-by-step).
                3. Các công thức toán học BẮT BUỘC viết dạng LaTeX đặt giữa dấu $.
                4. Output trả về định dạng JSON: {{ ""solutions"": [ {{ ""question_id"": ""Câu 1"", ""solution_steps"": ""..."", ""final_answer"": ""..."" }} ] }}";

            var payload = new
            {
                contents = new[] { new { parts = new object[] { new { text = prompt } } } },
                generationConfig = new { response_mime_type = "application/json" }
            };

            return await CallGeminiApi(payload);
        }

        // =========================================================
        // 3. GENERATE (TẠO ĐỀ MỚI TỪ JSON)
        // =========================================================
        public async Task<string> GenerateSimilarExam(string examStructureJson)
        {
            var prompt = $@"
                Bạn là chuyên gia ra đề thi. Hãy tạo một đề thi MỚI hoàn toàn dựa trên cấu trúc (Matrix) của đề thi cũ sau đây.
                
                INPUT FORMAT (JSON):
                {examStructureJson}

                YÊU CẦU:
                1. Giữ nguyên số lượng câu hỏi, mức độ khó và thang điểm.
                2. Thay đổi số liệu và ngữ cảnh câu hỏi (nhưng giữ nguyên dạng toán/logic).
                3. Công thức toán dùng LaTeX ($).
                4. Output trả về đúng cấu trúc JSON như Input.";

            var payload = new
            {
                contents = new[] { new { parts = new object[] { new { text = prompt } } } },
                generationConfig = new { response_mime_type = "application/json" }
            };

            return await CallGeminiApi(payload);
        }

        // =========================================================
        // HELPER: GỌI API CHUNG
        // =========================================================
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
                    throw new Exception($"Gemini Error ({response.StatusCode}): {errorBody}");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                // Parse để lấy text nội dung từ cấu trúc phức tạp của Gemini
                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "No content returned";
            }
            catch (Exception)
            {
                throw; // Rethrow để Controller xử lý
            }
        }
    }
}
