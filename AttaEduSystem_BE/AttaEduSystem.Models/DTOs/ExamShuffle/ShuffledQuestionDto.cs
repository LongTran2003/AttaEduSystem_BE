namespace AttaEduSystem.Models.DTOs.ExamShuffle
{
    public class ShuffledQuestionDto
    {
        public Guid OriginalQuestionId { get; set; }
        public int NewOrderIndex { get; set; }
        public string QuestionIdLabel { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public double? Points { get; set; }
        public string? CorrectAnswer { get; set; } // Đáp án đúng SAU KHI shuffle (VD: từ "A" -> "C")
        public List<ShuffledOptionDto> Options { get; set; } = new();
    }
}
