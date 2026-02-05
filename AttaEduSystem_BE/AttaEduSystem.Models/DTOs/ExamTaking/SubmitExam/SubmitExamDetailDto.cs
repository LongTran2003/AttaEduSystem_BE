namespace AttaEduSystem.Models.DTOs.SubmitExam;

public class SubmitExamDetailDto
{
    public Guid ExamQuestionId { get; set; }
    public string UserAnswer { get; set; } = string.Empty;
}