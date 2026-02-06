namespace AttaEduSystem.Models.DTOs.Statistics;

public class ProgressChartDto
{
    public DateTime Date { get; set; }
    public string DateLabel { get; set; } = string.Empty; // VD: "06/02"
    public double AverageScore { get; set; } // Điểm trung bình của ngày đó
    public int ExamCount { get; set; } // Số bài làm trong ngày
}