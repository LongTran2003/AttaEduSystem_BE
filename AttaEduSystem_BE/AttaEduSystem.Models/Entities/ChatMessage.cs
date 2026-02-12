using AttaEduSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ChatMessage : BaseEntity<string, string, string>
    {
        [Key]
        public Guid ChatMessageId { get; set; }

        [Required]
        public Guid ChatConversationId { get; set; }

        [Required]
        public MessageRole Role { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        // Navigation
        [ForeignKey("ChatConversationId")]
        public virtual ChatConversation? Conversation { get; set; }
    }
}
