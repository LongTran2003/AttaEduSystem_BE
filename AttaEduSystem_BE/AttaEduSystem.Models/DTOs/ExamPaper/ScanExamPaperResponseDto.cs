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

        /// <summary>True nếu đề thi đã có lời giải AI (do SolveImmediately=true hoặc giải sau)</summary>
        public bool HasSolution { get; set; }

        /// <summary>ID lời giải (null nếu chưa giải)</summary>
        public Guid? SolutionId { get; set; }
    }
}
