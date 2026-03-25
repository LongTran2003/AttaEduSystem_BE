using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.LearningClass;

public class CreateLearningClassDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(30)]
    public string? SchoolYear { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Schedule { get; set; }

    public List<string>? TeacherUserIds { get; set; }
    public List<string>? StudentUserIds { get; set; }
}
