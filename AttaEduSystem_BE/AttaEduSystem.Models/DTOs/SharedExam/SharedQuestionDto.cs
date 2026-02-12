namespace AttaEduSystem.Models.DTOs.SharedExam
{
    public class SharedQuestionDto
    {
        public int QuestionNumber { get; set; }
        public string Content { get; set; } = null!;
        public string QuestionType { get; set; } = "MultipleChoice";
        public List<string> Options { get; set; } = new();
        public string? CorrectAnswer { get; set; } // Only if IncludeAnswers = true
    }
}
