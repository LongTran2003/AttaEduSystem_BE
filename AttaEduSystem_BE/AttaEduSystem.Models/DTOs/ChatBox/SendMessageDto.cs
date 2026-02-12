using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ChatBox
{
    public class SendMessageDto
    {
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Content { get; set; } = null!;
    }
}
