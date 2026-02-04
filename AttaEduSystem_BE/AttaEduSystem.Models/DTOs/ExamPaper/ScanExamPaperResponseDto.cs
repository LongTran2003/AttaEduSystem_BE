using AttaEduSystem.Models.DTOs.ExamFormat;

namespace AttaEduSystem.Models.DTOs.ExamPaper
{
    public class ScanExamPaperResponseDto
    {
        public Guid ExamPaperId { get; set; }
        public string Title { get; set; } = null!;
        public string ScannedText { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string? ExamFormat { get; set; }
        public string? Subject { get; set; }
        public string? ScannedBy { get; set; }
        public DateTime ScannedAt { get; set; }
    }
}
