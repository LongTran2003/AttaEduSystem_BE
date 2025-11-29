namespace AttaEduSystem.Models.DTOs.Openai
{
    public class GenerateExamResponseDto
    {
        public Guid GeneratedExamId { get; set; }
        public string GeneratedContent { get; set; } = null!;
        public string AiModelUsed { get; set; } = null!;
        public string? PromptSnapshot { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
