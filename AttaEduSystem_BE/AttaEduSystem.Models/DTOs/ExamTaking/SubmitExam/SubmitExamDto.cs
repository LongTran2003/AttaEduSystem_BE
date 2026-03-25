namespace AttaEduSystem.Models.DTOs.SubmitExam;

public class SubmitExamDto
{
    public Guid? ExamAttemptId { get; set; }
    public Guid ExamPaperId { get; set; }
    public DateTime StartedAt { get; set; }
    public List<SubmitExamDetailDto> Answers { get; set; } = new();
}
