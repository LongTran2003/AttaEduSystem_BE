namespace AttaEduSystem.Models.DTOs.LearningClass;

public class LearningClassDetailDto : LearningClassSummaryDto
{
    public string OwnerUserId { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string? EnrollKey { get; set; }
    public List<LearningClassMemberDto> Members { get; set; } = new();
}
