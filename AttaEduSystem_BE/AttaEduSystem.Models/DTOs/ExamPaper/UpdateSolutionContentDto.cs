using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper
{
    /// <summary>
    /// Sửa nội dung đáp án thủ công (không dùng AI)
    /// </summary>
    public class UpdateSolutionContentDto
    {
        /// <summary>
        /// JSON nội dung lời giải do người dùng tự nhập/sửa.
        /// Giữ nguyên format JSON gốc của AI để FE parse nhất quán.
        /// </summary>
        [Required(ErrorMessage = "Solution content is required")]
        public string SolutionContentJson { get; set; } = null!;
    }
}
