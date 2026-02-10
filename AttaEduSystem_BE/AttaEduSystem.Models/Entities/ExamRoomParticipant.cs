using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ExamRoomParticipant : BaseEntity<string, string, string>
    {
        [Key]
        public Guid ParticipantId { get; set; } = Guid.NewGuid();

        public Guid ExamRoomId { get; set; }
        [ForeignKey("ExamRoomId")]
        public virtual ExamRoom ExamRoom { get; set; } = null!;

        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public DateTime JoinedAt { get; set; }

        // Link đến ExamAttempt khi bắt đầu làm bài
        public Guid? ExamAttemptId { get; set; }
        [ForeignKey("ExamAttemptId")]
        public virtual ExamAttempt? ExamAttempt { get; set; }
    }
}
