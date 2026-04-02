namespace AttaEduSystem.Models.DTOs.LearningClass;

public class LearningClassSummaryDto
{
    public Guid LearningClassId { get; set; }
    public string Name { get; set; } = null!;
    public string? SchoolYear { get; set; }
    public string? Description { get; set; }
    public string? Schedule { get; set; }
    public string Status { get; set; } = null!;
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int ExamRoomCount { get; set; }
    public bool HasEnrollKey { get; set; }
}
