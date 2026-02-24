namespace AttaEduSystem.Models.DTOs.ExamRoom.Room
{
    public class JoinExamRoomResponseDto
    {
        public Guid ParticipantId { get; set; }
        public Guid ExamRoomId { get; set; }
        public string RoomCode { get; set; } = null!;
        public Guid ExamPaperId { get; set; }
        public string? ExamTitle { get; set; }
        public int TimeLimit { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string RoomStatus { get; set; } = null!;
        public string ParticipantStatus { get; set; } = null!;
        public Guid? ExamAttemptId { get; set; }
        public int? RemainingSeconds { get; set; }
    }
}
