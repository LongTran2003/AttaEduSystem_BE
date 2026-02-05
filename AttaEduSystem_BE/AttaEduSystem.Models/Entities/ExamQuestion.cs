using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ExamQuestion : BaseEntity<string, string, string>
    {
        [Key]
        public Guid QuestionId { get; set; }

        public Guid ExamPaperId { get; set; }
        [ForeignKey("ExamPaperId")]
        public virtual ExamPaper ExamPaper { get; set; } = null!;

        public string Content { get; set; } = null!; // Nội dung câu hỏi
        public string? QuestionIdLabel { get; set; } // Ví dụ: "Câu 1", "Bài 1"
        public double? Points { get; set; }
        public int OrderIndex { get; set; }
        public string QuestionType { get; set; } = "Essay"; // "MultipleChoice" hoặc "Essay"
        
        [StringLength(10)] // VD: "A", "B"
        public string? CorrectAnswer { get; set; } 
    
        public string? DifficultyLevel { get; set; } // Easy, Medium, Hard (Chuẩn bị cho Phase Giáo viên)
        
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
