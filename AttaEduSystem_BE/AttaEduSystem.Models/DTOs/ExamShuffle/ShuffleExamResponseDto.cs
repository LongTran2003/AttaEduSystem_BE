namespace AttaEduSystem.Models.DTOs.ExamShuffle
{
    public class ShuffleExamResponseDto
    {
        public Guid OriginalExamPaperId { get; set; }
        public string OriginalExamTitle { get; set; } = null!;
        public int TotalVariants { get; set; }
        public List<ShuffledExamVariantDto> Variants { get; set; } = new();
    }
}
