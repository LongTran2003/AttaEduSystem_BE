using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Openai
{
    public class GenerateExamRequestDto
    {
        [Required]
        public Guid OriginalExamPaperId { get; set; }

        [Range(1, 20)]
        public int? NumberOfQuestions { get; set; }

        [StringLength(1000)]
        public string? CustomInstructions { get; set; }

        [StringLength(50)]
        public string? AiModel { get; set; }
    }
}
