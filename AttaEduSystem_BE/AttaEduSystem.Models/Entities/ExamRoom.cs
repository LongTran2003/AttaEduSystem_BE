using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class ExamRoom : BaseEntity<string, string, string>
    {
        [Key]
        public Guid ExamRoomId { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(6)]
        public string RoomCode { get; set; } = null!; // Random 6 ký tự (VD: "ABC123")

        public Guid ExamPaperId { get; set; }
        [ForeignKey("ExamPaperId")]
        public virtual ExamPaper ExamPaper { get; set; } = null!;

        public int TimeLimit { get; set; } // Thời gian làm bài (phút)
        public int MaxParticipants { get; set; } = 100; // Số người tối đa

        public DateTime StartTime { get; set; } // Thời gian bắt đầu
        public DateTime? EndTime { get; set; } // Thời gian kết thúc (nullable, tính từ StartTime + TimeLimit)

        // Navigation property
        public virtual ICollection<ExamRoomParticipant> Participants { get; set; } = new List<ExamRoomParticipant>();
    }
}
