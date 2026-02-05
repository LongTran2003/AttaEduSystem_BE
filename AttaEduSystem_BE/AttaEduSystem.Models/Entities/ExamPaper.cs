using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ExamPaper : BaseEntity<string, string, string>
    {
        [Key]
        public Guid ExamPaperId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string OriginalImageUrl { get; set; } = null!;

        public string? ScannedText { get; set; }

        public string? ExamFormat { get; set; } = "JSON";

        [StringLength(100)]
        public string? Subject { get; set; }

        public virtual ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
        
        [ForeignKey("CreatedBy")] 
        public virtual ApplicationUser? Creator { get; set; }
    }
}
