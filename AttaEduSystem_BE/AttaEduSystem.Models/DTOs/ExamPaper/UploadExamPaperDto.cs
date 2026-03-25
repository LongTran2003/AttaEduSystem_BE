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

        /// <summary>
        /// Nếu true: sau khi scan xong sẽ tự động gọi AI giải đề và trả về đáp án.
        /// Yêu cầu tài khoản Pro. Mặc định = false (chỉ hiện đề, không giải).
        /// </summary>
        public bool SolveImmediately { get; set; } = false;
    }
}

