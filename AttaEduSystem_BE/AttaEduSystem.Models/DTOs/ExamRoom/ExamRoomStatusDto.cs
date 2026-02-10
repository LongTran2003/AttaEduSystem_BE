namespace AttaEduSystem.Models.DTOs.ExamRoom
{
    public class ExamRoomStatusDto
    {
        public string RoomCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ExamTitle { get; set; }
        public int TimeLimit { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public int? RemainingSeconds { get; set; } // Số giây còn lại (null nếu chưa bắt đầu hoặc đã kết thúc)
        public bool CanJoin { get; set; }
        public string? Message { get; set; }
    }
}
