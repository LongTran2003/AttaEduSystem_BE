using AttaEduSystem.Models.Enums;

namespace AttaEduSystem.Models.DTOs.ChatBox
{
    public class ChatMessageDto
    {
        public Guid ChatMessageId { get; set; }
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime? CreatedTime { get; set; }
    }
}
