using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class SaveExamAnswerKeyDto
    {
        [Required]
        public List<SaveExamAnswerKeyItemDto> Questions { get; set; } = new();
    }

    public class SaveExamAnswerKeyItemDto
    {
        [Required]
        public Guid QuestionId { get; set; }

        [StringLength(20)]
        public string? QuestionType { get; set; }

        [StringLength(10)]
        public string? CorrectAnswer { get; set; }

        public List<UpdateQuestionOptionDto>? Options { get; set; }
    }
}
