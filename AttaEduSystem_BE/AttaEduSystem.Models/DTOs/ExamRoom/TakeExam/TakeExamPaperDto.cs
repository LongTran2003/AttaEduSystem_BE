namespace AttaEduSystem.Models.DTOs.ExamRoom.TakeExam
{
    public class TakeExamPaperDto
    {
        public Guid ExamPaperId { get; set; }
        public string Title { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public int TimeLimit { get; set; } // Lấy từ Room
        public Guid? ExamAttemptId { get; set; }
        public int? TimeRemainingSeconds { get; set; }
        public DateTime? LastSavedAt { get; set; }
        public bool CanUseAiSolve { get; set; }
        public int RemainingAiSolveQuota { get; set; }
        public List<TakeExamQuestionDto> Questions { get; set; } = new();
    }
}
