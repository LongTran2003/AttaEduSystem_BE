namespace AttaEduSystem.Models.DTOs.QuestionBank
{
    public class QuestionBankOptionDto
    {
        public Guid OptionId { get; set; }
        public string OptionLabel { get; set; } = null!;
        public string OptionContent { get; set; } = null!;
    }
}
