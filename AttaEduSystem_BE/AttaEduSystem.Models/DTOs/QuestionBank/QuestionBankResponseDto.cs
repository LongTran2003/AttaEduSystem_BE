namespace AttaEduSystem.Models.DTOs.QuestionBank
{
    public class QuestionBankResponseDto
    {
        public List<QuestionBankItemDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
