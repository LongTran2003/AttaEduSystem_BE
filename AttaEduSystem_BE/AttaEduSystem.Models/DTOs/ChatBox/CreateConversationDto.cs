using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ChatBox
{
    public class CreateConversationDto
    {
        /// <summary>
        /// Tin nhắn đầu tiên gửi cho AI
        /// </summary>
        [Required(ErrorMessage = "Message is required")]
        [StringLength(10000, MinimumLength = 1)]
        public string Message { get; set; } = null!;
    }
}
