namespace AttaEduSystem.Models.DTOs.ExamFormat
{
    public class ExamSection
    {
        public string Name { get; set; } = string.Empty;
        public decimal? TotalPoints { get; set; }
        public List<ExamQuestionFormatDto> Questions { get; set; } = new();
    }
}
