using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper
{
    public class UploadExamPaperDto
    {
        [Required(ErrorMessage = "Exam image is required")]
        public IFormFile ExamImage { get; set; } = null!;

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Subject { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
