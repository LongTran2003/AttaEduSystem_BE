namespace AttaEduSystem.Models.DTOs.ExamRoom.TakeExam
{
    public class TakeExamPaperDto
    {
        public Guid ExamPaperId { get; set; }
        public string Title { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public int TimeLimit { get; set; } // Lấy từ Room
        public List<TakeExamQuestionDto> Questions { get; set; } = new();
    }
}
