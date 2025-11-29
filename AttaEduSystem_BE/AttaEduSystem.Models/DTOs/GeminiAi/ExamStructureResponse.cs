namespace AttaEduSystem.Models.DTOs.GeminiAi
{
    public class ExamStructureResponse
    {
        public ExamInfo Exam_info { get; set; } = null!;
        public List<QuestionItem> Questions { get; set; } = new List<QuestionItem>();
    }
}
