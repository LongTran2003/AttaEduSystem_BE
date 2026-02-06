namespace AttaEduSystem.Models.DTOs.Statistics;

public class OverviewStatsDto
{
    public int TotalExamsTaken { get; set; } // Tổng số bài đã làm
    public double AverageScore { get; set; } // Điểm trung bình
    public double HighestScore { get; set; } // Điểm cao nhất
    public int TotalQuestionsAnswered { get; set; } // Tổng số câu đã trả lời
}