using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class ReorderQuestionsDto
    {
        [Required]
        public List<QuestionOrderItem> QuestionOrders { get; set; } = new();
    }
}
