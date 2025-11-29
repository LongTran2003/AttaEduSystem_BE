using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class QuestionOption : BaseEntity<string, string, string>
    {
        [Key]
        public Guid OptionId { get; set; } = Guid.NewGuid();

        public Guid QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual ExamQuestion Question { get; set; } = null!;

        public string Label { get; set; } = null!; // A, B, C, D
        public string Content { get; set; } = null!; // Nội dung đáp án
        public bool IsCorrect { get; set; } = false;
    }
}
