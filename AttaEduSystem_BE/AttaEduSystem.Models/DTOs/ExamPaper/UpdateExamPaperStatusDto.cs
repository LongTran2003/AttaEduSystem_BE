using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper
{
    public class UpdateExamPaperStatusDto
    {
        [Required]
        [RegularExpression("Draft|Ready|Removed")]
        public string Status { get; set; } = null!;
    }
}
