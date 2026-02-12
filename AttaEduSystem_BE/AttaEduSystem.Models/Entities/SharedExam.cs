using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class SharedExam : BaseEntity<string, string, string>
    {
        [Key]
        public Guid SharedExamId { get; set; }

        /// <summary>
        /// Unique token for sharing (8-12 chars)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ShareToken { get; set; } = null!;

        /// <summary>
        /// Source type: "ExamPaper" or "GeneratedExam"
        /// </summary>
        [Required]
        [StringLength(20)]
        public string SourceType { get; set; } = "ExamPaper";

        /// <summary>
        /// ID of ExamPaper or GeneratedExamPaper
        /// </summary>
        [Required]
        public Guid SourceId { get; set; }

        /// <summary>
        /// Optional expiration date (null = never expires)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Optional password protection (null = no password)
        /// </summary>
        [StringLength(100)]
        public string? Password { get; set; }

        /// <summary>
        /// Maximum number of views allowed (null = unlimited)
        /// </summary>
        public int? MaxViews { get; set; }

        /// <summary>
        /// Current view count
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Is the share link active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Include answers when viewing
        /// </summary>
        public bool IncludeAnswers { get; set; } = false;

        // Navigation (optional, for ExamPaper only)
        [ForeignKey("SourceId")]
        public virtual ExamPaper? ExamPaper { get; set; }
    }
}
