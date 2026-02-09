using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class UpdateQuestionOptionDto
    {
        public Guid? OptionId { get; set; } // Null nếu là option mới

        [Required]
        public string OptionLabel { get; set; } = null!; // "A", "B", "C", "D"

        [Required]
        public string OptionContent { get; set; } = null!;
    }
}
