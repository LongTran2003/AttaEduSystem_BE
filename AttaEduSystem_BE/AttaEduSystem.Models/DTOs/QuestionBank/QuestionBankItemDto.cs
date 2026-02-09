namespace AttaEduSystem.Models.DTOs.QuestionBank
{
    public class QuestionBankItemDto
    {
        public Guid QuestionId { get; set; }
        public Guid ExamPaperId { get; set; }
        public string ExamPaperTitle { get; set; } = null!;
        public string? Subject { get; set; }
        public string Content { get; set; } = null!;
        public string? QuestionIdLabel { get; set; }
        public string QuestionType { get; set; } = null!;
        public string? DifficultyLevel { get; set; }
        public double? Points { get; set; }
        public string? CorrectAnswer { get; set; }
        public List<QuestionBankOptionDto> Options { get; set; } = new();
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
    }
}
