using System.Text.Json.Serialization;

namespace AttaEduSystem.Models.DTOs.GeminiAi
{
    public class ExamStructureResponse
    {
        [JsonPropertyName("exam_info")]
        public Dictionary<string, string>? ExamInfo { get; set; }

        [JsonPropertyName("questions")]
        public List<QuestionItem>? Questions { get; set; }
    }
}
