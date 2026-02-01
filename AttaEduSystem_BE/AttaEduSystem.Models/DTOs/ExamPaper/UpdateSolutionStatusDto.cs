using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper;

public class UpdateSolutionStatusDto
{
    [Required]
    [RegularExpression("Saved|Deleted", ErrorMessage = "Status must be: Saved or Deleted")]
    public string Status { get; set; } = null!;
}