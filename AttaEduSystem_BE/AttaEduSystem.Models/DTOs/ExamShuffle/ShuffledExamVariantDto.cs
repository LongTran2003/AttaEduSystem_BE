namespace AttaEduSystem.Models.DTOs.ExamShuffle
{
    public class ShuffledExamVariantDto
    {
        public Guid VariantId { get; set; }
        public string VariantCode { get; set; } = null!; // "A", "B", "C", "D"...
        public Guid OriginalExamPaperId { get; set; }
        public string OriginalExamTitle { get; set; } = null!;
        public List<ShuffledQuestionDto> Questions { get; set; } = new();
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
