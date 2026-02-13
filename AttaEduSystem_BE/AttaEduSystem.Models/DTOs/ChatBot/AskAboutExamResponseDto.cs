using AttaEduSystem.Models.DTOs.ChatBox;

namespace AttaEduSystem.Models.DTOs.ChatBot
{
    public class AskAboutExamResponseDto
    {
        public Guid ConversationId { get; set; }
        public string? ConversationTitle { get; set; }
        public Guid ExamPaperId { get; set; }
        public string? ExamTitle { get; set; }
        public ChatMessageDto UserMessage { get; set; } = null!;
        public ChatMessageDto AiResponse { get; set; } = null!;
    }
}
