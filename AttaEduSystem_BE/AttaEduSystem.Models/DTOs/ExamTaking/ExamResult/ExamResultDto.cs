namespace AttaEduSystem.Models.DTOs.ExamResult;

public class ExamResultDto
{
    public Guid ExamAttemptId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime CompletedAt { get; set; }
    public List<ExamResultDetailDto> Details { get; set; } = new();
}