namespace AttaEduSystem.Models.DTOs.Openai
{
    public class GeneratedExamDto
    {
        public Guid GeneratedExamId { get; set; }
        public Guid OriginalExamPaperId { get; set; }
        public string GeneratedContent { get; set; } = null!;
        public string AiModelUsed { get; set; } = null!;
        public string? Status { get; set; }
        public string? GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }

    }
}
