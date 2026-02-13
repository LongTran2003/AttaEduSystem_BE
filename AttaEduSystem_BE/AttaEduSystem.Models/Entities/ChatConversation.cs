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
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public Guid? ExamPaperId { get; set; }
        [ForeignKey("ExamPaperId")]
        public virtual ExamPaper? ExamPaper { get; set; }

        [StringLength(200)]
        public string? Title { get; set; } // Auto-generated từ tin nhắn đầu tiên
        public DateTime? UpdatedAt { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Deleted

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
