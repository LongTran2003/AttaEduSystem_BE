namespace AttaEduSystem.Models.DTOs.ExamFormat
{
    public class ExamQuestion
    {
        public string Code { get; set; } = string.Empty; // Câu 1, Câu 2...
        public string Requirement { get; set; } = string.Empty;
        public decimal? Points { get; set; }
        public string? SubRequirement { get; set; } // ví dụ “Câu hỏi nhỏ” nếu có
    }
}
