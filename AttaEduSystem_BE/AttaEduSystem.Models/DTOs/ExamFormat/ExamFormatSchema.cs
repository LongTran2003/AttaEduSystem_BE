namespace AttaEduSystem.Models.DTOs.ExamFormat
{
    public class ExamFormatSchema
    {
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? TimeLimit { get; set; }
        public string? Instructions { get; set; }
        public List<ExamSection> Sections { get; set; } = new();
    }
}
