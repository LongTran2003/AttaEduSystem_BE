using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamShuffle
{
    public class ShuffleExamRequestDto
    {
        [Range(1, 10, ErrorMessage = "Number of variants must be between 1 and 10")]
        public int NumberOfVariants { get; set; } = 1;

        public bool ShuffleQuestions { get; set; } = true;

        public bool ShuffleOptions { get; set; } = true;
    }
}
