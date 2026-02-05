using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities;

public class ExamAttempt : BaseEntity<string, string, string>
{
    [Key]
    public Guid ExamAttemptId { get; set; }

    public Guid ExamPaperId { get; set; }
    [ForeignKey("ExamPaperId")]
    public virtual ExamPaper ExamPaper { get; set; } = null!;

    public string UserId { get; set; } = null!;
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public double Score { get; set; } // Điểm số (VD: 8.5)
    public int CorrectCount { get; set; } // Số câu đúng
    public int TotalQuestions { get; set; } // Tổng số câu

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<ExamAttemptDetail> Details { get; set; } = new List<ExamAttemptDetail>();
}