namespace AttaEduSystem.Models.DTOs.ExamTaking;

public class ExamAttemptLiveStatusDto
{
    public Guid ExamAttemptId { get; set; }
    public int AnsweredCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime? LastSavedAt { get; set; }
    public int? TimeRemainingSeconds { get; set; }
}
