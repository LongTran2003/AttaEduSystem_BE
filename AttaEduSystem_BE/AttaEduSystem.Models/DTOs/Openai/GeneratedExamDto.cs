namespace AttaEduSystem.Models.DTOs.Openai
{
    public class GeneratedExamDto
    {
        public Guid GeneratedExamId { get; set; }
        public string GeneratedContent { get; set; } = null!;
        public string AiModelUsed { get; set; } = null!;
        public DateTime GeneratedAt { get; set; }
    }
}
