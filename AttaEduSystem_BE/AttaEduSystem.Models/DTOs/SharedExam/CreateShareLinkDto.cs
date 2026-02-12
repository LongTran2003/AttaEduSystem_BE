using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.SharedExam
{
    public class CreateShareLinkDto
    {
        /// <summary>
        /// Source type: "ExamPaper" or "GeneratedExam"
        /// </summary>
        [Required]
        public string SourceType { get; set; } = "ExamPaper";

        /// <summary>
        /// ID of ExamPaper or GeneratedExamPaper
        /// </summary>
        [Required]
        public Guid SourceId { get; set; }

        /// <summary>
        /// Expiration in days (null = never expires)
        /// </summary>
        public int? ExpiresInDays { get; set; }

        /// <summary>
        /// Optional password protection
        /// </summary>
        [StringLength(50)]
        public string? Password { get; set; }

        /// <summary>
        /// Maximum views allowed (null = unlimited)
        /// </summary>
        public int? MaxViews { get; set; }

        /// <summary>
        /// Include answers when viewing
        /// </summary>
        public bool IncludeAnswers { get; set; } = false;
    }
}
