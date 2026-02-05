using System.Text.Json.Serialization;

namespace AttaEduSystem.Models.DTOs.GeminiAi
{
    public class QuestionItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Essay"; // Mặc định là Essay nếu null

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public List<string>? Options { get; set; }

        [JsonPropertyName("points")]
        public double Points { get; set; }
    }
}
