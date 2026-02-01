using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper;

public class UpdateGeneratedExamStatusDto
{
    [Required]
    // Chỉ cho phép các trạng thái này
    [RegularExpression("Draft|Saved|Deleted", ErrorMessage = "Status must be: Draft, Saved, or Deleted")]
    public string Status { get; set; } = null!;
}