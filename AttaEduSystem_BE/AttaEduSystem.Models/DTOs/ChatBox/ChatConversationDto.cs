namespace AttaEduSystem.Models.DTOs.ChatBox
{
    public class ChatConversationDto
    {
        public Guid ChatConversationId { get; set; }
        public string? Title { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int MessageCount { get; set; }
        public Guid? ExamPaperId { get; set; }
        public string? ExamTitle { get; set; }
    }
}
