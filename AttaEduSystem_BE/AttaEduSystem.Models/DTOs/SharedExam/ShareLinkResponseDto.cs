namespace AttaEduSystem.Models.DTOs.SharedExam
{
    public class ShareLinkResponseDto
    {
        public Guid SharedExamId { get; set; }
        public string ShareToken { get; set; } = null!;
        public string ShareUrl { get; set; } = null!;
        public string SourceType { get; set; } = null!;
        public Guid SourceId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool HasPassword { get; set; }
        public int? MaxViews { get; set; }
        public bool IncludeAnswers { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
