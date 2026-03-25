using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.LearningClass;

public class UpdateLearningClassDto
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(30)]
    public string? SchoolYear { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Schedule { get; set; }

    public string? Status { get; set; }
}
