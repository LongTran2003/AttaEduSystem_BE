using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper
{
    public class UpdateExamPaperStatusDto
    {
        
        [RegularExpression("Draft|Ready|Removed", ErrorMessage = "Status must be Draft, Ready, or Removed")]
        public string? Status { get; set; }

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Subject cannot exceed 100 characters")]
        public string? Subject { get; set; }
    }
}
