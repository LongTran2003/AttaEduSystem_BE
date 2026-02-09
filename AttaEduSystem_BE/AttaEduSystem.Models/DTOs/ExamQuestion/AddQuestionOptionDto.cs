using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class AddQuestionOptionDto
    {
        [Required]
        public string OptionLabel { get; set; } = null!;

        [Required]
        public string OptionContent { get; set; } = null!;
    }
}
