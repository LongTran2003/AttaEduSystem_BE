namespace AttaEduSystem.Models.DTOs.ChatBox
{
    public class ConversationDetailDto
    {
        public Guid ChatConversationId { get; set; }
        public string? Title { get; set; }
        public DateTime? CreatedTime { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }
}
