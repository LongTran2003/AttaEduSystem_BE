namespace AttaEduSystem.Models.DTOs.SharedExam
{
    public class MyShareLinkDto
    {
        public Guid SharedExamId { get; set; }
        public string ShareToken { get; set; } = null!;
        public string SourceType { get; set; } = null!;
        public string ExamTitle { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public bool HasPassword { get; set; }
        public int ViewCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
