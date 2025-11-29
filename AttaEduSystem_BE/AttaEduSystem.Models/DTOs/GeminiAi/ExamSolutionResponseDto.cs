namespace AttaEduSystem.Models.DTOs.GeminiAi
{
    public class ExamSolutionResponseDto
    {
        public Guid ExamSolutionId { get; set; }
        public Guid ExamPaperId { get; set; }
        public string SolutionContentJson { get; set; } = null!;
        public DateTime SolvedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
