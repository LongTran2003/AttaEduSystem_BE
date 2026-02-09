using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class AddExamQuestionDto
    {
        [Required]
        public string Content { get; set; } = null!;

        public string? QuestionIdLabel { get; set; }

        public double? Points { get; set; }

        [StringLength(20)]
        public string QuestionType { get; set; } = "Essay";

        [StringLength(10)]
        public string? CorrectAnswer { get; set; }

        public string? DifficultyLevel { get; set; }

        public List<AddQuestionOptionDto>? Options { get; set; }
    }
}
