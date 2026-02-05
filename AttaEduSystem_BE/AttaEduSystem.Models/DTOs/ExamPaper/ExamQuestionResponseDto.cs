namespace AttaEduSystem.Models.DTOs.ExamPaper;

public class ExamQuestionResponseDto
{
    public Guid QuestionId { get; set; } // Đây là cái Frontend cần để Submit
    public string Content { get; set; } = string.Empty;
    public string? QuestionIdLabel { get; set; } // "Câu 1"
    public int OrderIndex { get; set; }
    public double? Points { get; set; }
    public string QuestionType { get; set; } = "Essay";
    public List<QuestionOptionDto> Options { get; set; } = new();
}