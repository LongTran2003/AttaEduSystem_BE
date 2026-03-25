namespace AttaEduSystem.Models.DTOs.LearningClass;

public class AddLearningClassMembersDto
{
    public List<string>? TeacherUserIds { get; set; }
    public List<string>? StudentUserIds { get; set; }
}
