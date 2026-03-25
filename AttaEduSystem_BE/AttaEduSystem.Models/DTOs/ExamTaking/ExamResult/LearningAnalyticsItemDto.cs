namespace AttaEduSystem.Models.DTOs.ExamResult;

public class LearningAnalyticsItemDto
{
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int WrongCount { get; set; }
}
