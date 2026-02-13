using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ChatBot
{
    public class AskAboutExamDto
    {
        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = null!;

        /// <summary>
        /// Optional: Nếu muốn tiếp tục conversation hiện tại thay vì tạo mới
        /// </summary>
        public Guid? ConversationId { get; set; }

        /// <summary>
        /// Optional: Có bao gồm lời giải (ExamSolution) vào context không?
        /// </summary>
        public bool IncludeSolutions { get; set; } = false;
    }
}
