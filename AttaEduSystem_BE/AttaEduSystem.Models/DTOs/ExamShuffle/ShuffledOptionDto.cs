namespace AttaEduSystem.Models.DTOs.ExamShuffle
{
    public class ShuffledOptionDto
    {
        public string OriginalLabel { get; set; } = null!; // Label gốc: "A"
        public string NewLabel { get; set; } = null!; // Label mới sau shuffle: "C"
        public string Content { get; set; } = null!;
    }
}
