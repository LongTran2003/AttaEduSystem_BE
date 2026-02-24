namespace AttaEduSystem.Models.DTOs.ExamRoom.Room
{
    public class ExamRoomResponseDto
    {
        public Guid ExamRoomId { get; set; }
        public string RoomCode { get; set; } = null!;
        public Guid ExamPaperId { get; set; }
        public string? ExamTitle { get; set; }
        public int TimeLimit { get; set; }
        public int MaxParticipants { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = null!;
        public int CurrentParticipants { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
