using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.LearningClass;

public class JoinClassByEnrollKeyDto
{
    [Required]
    [StringLength(20)]
    public string EnrollKey { get; set; } = string.Empty;
}
