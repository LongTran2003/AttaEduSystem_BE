namespace AttaEduSystem.Models.DTOs.GeminiAi
{
    public class QuestionItem
    {
        public string Id { get; set; } = null!;
        public string Content { get; set; } = null!;
        public double? Points { get; set; }
        public List<string>? Options { get; set; }
    }
}
