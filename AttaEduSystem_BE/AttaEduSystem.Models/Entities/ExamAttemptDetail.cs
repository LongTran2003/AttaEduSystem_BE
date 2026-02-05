using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities;

public class ExamAttemptDetail : BaseEntity<string, string, string>
{
    [Key]
    public Guid ExamAttemptDetailId { get; set; }

    public Guid ExamAttemptId { get; set; }
    [ForeignKey("ExamAttemptId")]
    public virtual ExamAttempt ExamAttempt { get; set; } = null!;

    public Guid ExamQuestionId { get; set; }
    [ForeignKey("ExamQuestionId")]
    public virtual ExamQuestion ExamQuestion { get; set; } = null!;

    public string? UserAnswer { get; set; } // VD: "A", "B" hoặc text
    public bool IsCorrect { get; set; }
}