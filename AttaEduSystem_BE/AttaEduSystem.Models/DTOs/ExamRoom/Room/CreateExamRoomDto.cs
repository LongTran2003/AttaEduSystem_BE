using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamRoom.Room
{
    public class CreateExamRoomDto
    {
        [Required]
        public Guid ExamPaperId { get; set; }

        [Required]
        [Range(1, 480, ErrorMessage = "Time limit must be between 1 and 480 minutes")]
        public int TimeLimit { get; set; } // Thời gian làm bài (phút)

        [Required]
        public DateTime StartTime { get; set; } // Thời gian bắt đầu

        [Range(1, 100, ErrorMessage = "Max participants must be between 1 and 100")]
        public int MaxParticipants { get; set; } = 100;
    }
}
