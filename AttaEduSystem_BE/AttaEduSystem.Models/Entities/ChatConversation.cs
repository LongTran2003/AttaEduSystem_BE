using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ChatConversation : BaseEntity<string, string, string>
    {
        [Key]
        public Guid ChatConversationId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [StringLength(200)]
        public string? Title { get; set; } // Auto-generated từ tin nhắn đầu tiên

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
