namespace AttaEduSystem.Models.DTOs.LearningClass;

public class LearningClassMemberDto
{
    public Guid LearningClassMemberId { get; set; }
    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}
