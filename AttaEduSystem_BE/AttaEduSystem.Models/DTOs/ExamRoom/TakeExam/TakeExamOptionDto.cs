namespace AttaEduSystem.Models.DTOs.ExamRoom.TakeExam
{
    public class TakeExamOptionDto
    {
        public Guid OptionId { get; set; }
        public string Label { get; set; } = null!;
        public string Content { get; set; } = null!;
        // 🔴 KHÔNG CÓ IsCorrect
    }
}
