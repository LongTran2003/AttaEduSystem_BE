namespace AttaEduSystem.Models.DTOs.ExamRoom.TakeExam
{
    public class TakeExamQuestionDto
    {
        public Guid QuestionId { get; set; }
        public string Content { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public decimal Points { get; set; }
        public int OrderIndex { get; set; }
        // 🔴 KHÔNG CÓ CorrectAnswer
        public List<TakeExamOptionDto> Options { get; set; } = new();
    }
}
