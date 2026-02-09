using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.QuestionBank
{
    public class QuestionBankFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? QuestionType { get; set; } // "MultipleChoice", "Essay"
        public string? DifficultyLevel { get; set; } // "Easy", "Medium", "Hard"
        public string? Subject { get; set; }

        [StringLength(10)]
        public string Scope { get; set; } = "All"; // "My", "Public", "All"

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}
