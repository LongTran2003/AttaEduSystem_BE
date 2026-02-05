namespace AttaEduSystem.Models.DTOs.ExamResult;

public class ExamResultDetailDto
{
    public Guid ExamQuestionId { get; set; }
    public string QuestionContent { get; set; } = string.Empty; // Nội dung câu hỏi
    public int QuestionIndex { get; set; } // Câu số 1, 2...
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; } // Lời giải thích (nếu có sau này)
}