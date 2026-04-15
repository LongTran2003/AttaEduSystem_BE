using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AttaEduSystem.Services.Services
{
    public class GeminiAiService : IGeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string[] _models;

        public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = ResolveGeminiApiKey(configuration);
            _models = ResolveGeminiModels(configuration);
        }

        // =========================================================
        // 1. ANALYZE (PHÂN TÍCH ĐỀ TỪ ẢNH)
        // =========================================================
        public async Task<string> AnalyzeExamStructure(string base64Image, string mimeType)
        {
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
        // 4. GENERATE STUDY PLAN (TẠO LỊCH HỌC TỪ KẾT QUẢ HỌC TẬP)
        // =========================================================
        public async Task<string> GenerateStudyPlan(string subjectPerformanceJson, string preferencesJson)
        {
            var prompt = $@"
        Bạn là chuyên gia tư vấn học tập. Hãy tạo lịch học tuần cho sinh viên dựa trên:

        📊 KẾT QUẢ HỌC TẬP GẦN ĐÂY:
        {subjectPerformanceJson}

        🎯 YÊU CẦU CỦA SINH VIÊN:
        {preferencesJson}

        📝 YÊU CẦU OUTPUT:
        1. Phân tích điểm yếu và đề xuất thứ tự ưu tiên
        2. Lịch học 7 ngày (Monday -> Sunday) với:
           - Mỗi ngày: Môn học, mục tiêu cụ thể, thời lượng, lý do
           - Cân đối giữa các môn, tránh quá tải
           - Ưu tiên môn yếu nhưng không bỏ qua môn mạnh hoàn toàn
        3. Gợi ý tài nguyên học tập (nếu có thể)
        4. Đánh giá confidence (0-100) về hiệu quả của plan

        OUTPUT JSON SCHEMA (STRICT):
        {{
            ""summary"": ""Tóm tắt tình hình và chiến lược học tập"",
            ""performance"": {{
                ""weakSubjects"": [
                    {{
                        ""subject"": ""Toán"",
                        ""averageScore"": 5.5,
                        ""attemptCount"": 3,
                        ""trend"": ""Declining"",
                        ""priority"": 1,
                        ""correctRate"": 55.0
                    }}
                ],
                ""strongSubjects"": [...]
            }},
            ""dailyPlans"": [
                {{
                    ""dayOfWeek"": ""Monday"",
                    ""date"": ""2026-03-02"",
                    ""sessions"": [
                        {{
                            ""sessionOrder"": 1,
                            ""subject"": ""Toán"",
                            ""goal"": ""Ôn lại phần hàm số bậc 2, làm 10 bài tập"",
                            ""duration"": 1.5,
                            ""reasoning"": ""Điểm TB 5.5, xu hướng giảm - cần ưu tiên cao nhất"",
                            ""suggestedResources"": [""Bài tập SGK chương 3"", ""Video Khan Academy - Quadratic Functions""],
                            ""isCompleted"": false,
                            ""completionNotes"": null,
                            ""completedAt"": null
                        }}
                    ],
                    ""totalHours"": 2.0
                }}
            ],
            ""confidenceScore"": 85
        }}

        QUY TẮC QUAN TRỌNG:
        - Nếu không có dữ liệu học tập, tạo plan chung chung dựa trên môn ưu tiên (nếu có)
        - Mỗi ngày PHẢI có ít nhất 1 session (không để trống)
        - Tổng giờ học mỗi ngày không vượt quá yêu cầu
        - Trend chỉ có 3 giá trị: ""Improving"", ""Declining"", ""Stable""
        - Priority từ 1-5 (1 = cao nhất)

        CHỈ TRẢ VỀ JSON THUẦN, KHÔNG MARKDOWN.
    ";

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
            var lastError = "Unknown Gemini error";

            foreach (var model in _models)
            {
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

                for (var attempt = 1; attempt <= 3; attempt++)
                {
                    using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseString);
                        var text = doc.RootElement.GetProperty("candidates")[0]
                            .GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        return text ?? "No content returned";
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();
                    lastError = $"Gemini Error ({response.StatusCode}): {errorBody}";

                    var isQuota = response.StatusCode == HttpStatusCode.TooManyRequests ||
                                  errorBody.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase) ||
                                  errorBody.Contains("quota", StringComparison.OrdinalIgnoreCase);
                    var isTransient = response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                      response.StatusCode == HttpStatusCode.BadGateway ||
                                      response.StatusCode == HttpStatusCode.GatewayTimeout;

                    if ((isQuota || isTransient) && attempt < 3)
                    {
                        var delay = ExtractRetryDelaySeconds(errorBody) ?? (2 * attempt);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, delay)));
                        continue;
                    }

                    if (isQuota)
                        break;

                    throw new Exception(lastError);
                }
            }

            throw new Exception($"GEMINI_QUOTA_EXCEEDED|{lastError}");
        }

        private static int? ExtractRetryDelaySeconds(string errorBody)
        {
            var match = Regex.Match(errorBody, @"retryDelay""\s*:\s*""(?<sec>\d+)s""", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups["sec"].Value, out var sec))
                return sec;

            var fallback = Regex.Match(errorBody, @"retry in\s+(?<sec>\d+(?:\.\d+)?)s", RegexOptions.IgnoreCase);
            if (fallback.Success && double.TryParse(fallback.Groups["sec"].Value, out var sec2))
                return (int)Math.Ceiling(sec2);

            return null;
        }

        private static string ResolveGeminiApiKey(IConfiguration configuration)
        {
            var apiKey = new[]
            {
                configuration["Gemini:SolveApiKey"],
                configuration["Gemini__SolveApiKey"],
                configuration["Gemini_SolveApiKey"],
                configuration["Gemini:ApiKey"],
                configuration["Gemini__ApiKey"],
                configuration["Gemini_ApiKey"],
                configuration["Gemini_Apikey"]
            }.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini API key missing");

            return apiKey;
        }

        private static string[] ResolveGeminiModels(IConfiguration configuration)
        {
            var primary = configuration["Gemini:SolveModel"] ?? "gemini-2.5-flash";
            var fallback = configuration["Gemini:SolveFallbackModel"] ?? "gemini-1.5-flash";
            return new[] { primary.Trim(), fallback.Trim() }
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
