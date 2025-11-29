using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class GeneratedExamPaper : BaseEntity<string, string, string>
    {
        [Key]
        public Guid GeneratedExamPaperId { get; set; }

        [Required]
        public Guid OriginalExamPaperId { get; set; }

        [ForeignKey(nameof(OriginalExamPaperId))]
        public ExamPaper OriginalExamPaper { get; set; } = null!;

        [Required]
        public string GeneratedContentJson { get; set; } = null!;

        [StringLength(100)]
        public string AiModelUsed { get; set; } = null!;

        [StringLength(2000)]
        public string? PromptSnapshot { get; set; }
    }
}
