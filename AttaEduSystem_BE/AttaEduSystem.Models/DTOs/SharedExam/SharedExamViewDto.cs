namespace AttaEduSystem.Models.DTOs.SharedExam
{
    public class SharedExamViewDto
    {
        public string Title { get; set; } = null!;
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string SharedBy { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public int ViewCount { get; set; }
        public List<SharedQuestionDto> Questions { get; set; } = new();

        // Export options
        public bool CanExportPdf { get; set; } = true;
        public bool CanExportWord { get; set; } = true;
    }
}
