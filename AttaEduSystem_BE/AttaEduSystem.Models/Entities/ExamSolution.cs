using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ExamSolution : BaseEntity<string, string, string>
    {
        [Key]
        public Guid ExamSolutionId { get; set; } = Guid.NewGuid();

        public Guid ExamPaperId { get; set; }
        [ForeignKey("ExamPaperId")]
        public virtual ExamPaper ExamPaper { get; set; } = null!;

        // Lưu toàn bộ JSON lời giải mà Gemini trả về
        public string SolutionContentJson { get; set; } = null!;

    }
}
