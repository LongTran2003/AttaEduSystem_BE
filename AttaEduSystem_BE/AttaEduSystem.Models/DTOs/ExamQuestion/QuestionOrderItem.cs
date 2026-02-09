using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class QuestionOrderItem
    {
        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OrderIndex { get; set; }
    }
}
