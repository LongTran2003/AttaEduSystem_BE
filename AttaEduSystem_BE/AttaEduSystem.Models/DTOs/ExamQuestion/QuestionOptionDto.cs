namespace AttaEduSystem.Models.DTOs.ExamQuestion
{
    public class QuestionOptionDto
    {
        public Guid OptionId { get; set; }
        public string OptionLabel { get; set; } = null!;
        public string OptionContent { get; set; } = null!;
    }
}
