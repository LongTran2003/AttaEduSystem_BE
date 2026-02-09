using AttaEduSystem.Models.DTOs.ExamPaper;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class ExamQuestionDto
    {
        public Guid QuestionId { get; set; }
        public Guid ExamPaperId { get; set; }
        public string Content { get; set; } = null!;
        public string? QuestionIdLabel { get; set; }
        public double? Points { get; set; }
        public int OrderIndex { get; set; }
        public string QuestionType { get; set; } = "Essay";
        public string? CorrectAnswer { get; set; }
        public string? DifficultyLevel { get; set; }
        public List<QuestionOptionDto>? Options { get; set; }
    }
}
