namespace AttaEduSystem.Models.DTOs.ExamTaking;

public class ExamHistoryDto
{
    public Guid ExamAttemptId { get; set; }
    public Guid ExamPaperId { get; set; }
    public string ExamTitle { get; set; } = string.Empty; // Tên đề thi
    public string Subject { get; set; } = string.Empty;
    public double Score { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Duration { get; set; } = string.Empty; // VD: "45 mins"
}