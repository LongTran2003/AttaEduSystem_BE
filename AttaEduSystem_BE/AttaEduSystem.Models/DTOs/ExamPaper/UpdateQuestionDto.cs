using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ExamPaper
{
    /// <summary>
    /// Sửa nội dung 1 câu hỏi trong đề thi đã scan
    /// </summary>
    public class UpdateQuestionDto
    {
        /// <summary>Nội dung câu hỏi (để trống = giữ nguyên)</summary>
        [StringLength(2000)]
        public string? Content { get; set; }

        /// <summary>Nhãn câu hỏi, ví dụ: "Câu 1", "Bài 2" (để trống = giữ nguyên)</summary>
        [StringLength(50)]
        public string? QuestionIdLabel { get; set; }

        /// <summary>Điểm số (null = giữ nguyên)</summary>
        [Range(0, 100)]
        public double? Points { get; set; }

        /// <summary>Đáp án đúng (chỉ dùng cho trắc nghiệm, VD: "A", "B", "C", "D")</summary>
        [StringLength(10)]
        public string? CorrectAnswer { get; set; }

        /// <summary>Loại câu hỏi: "Essay" hoặc "MultipleChoice"</summary>
        [RegularExpression("Essay|MultipleChoice", ErrorMessage = "QuestionType must be Essay or MultipleChoice")]
        public string? QuestionType { get; set; }
    }

    /// <summary>
    /// Sửa nhiều câu hỏi cùng lúc (batch update)
    /// </summary>
    public class BatchUpdateQuestionsDto
    {
        [Required]
        public List<QuestionUpdateItem> Questions { get; set; } = new();
    }

    public class QuestionUpdateItem
    {
        [Required]
        public Guid QuestionId { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? QuestionIdLabel { get; set; }

        [Range(0, 100)]
        public double? Points { get; set; }

        [StringLength(10)]
        public string? CorrectAnswer { get; set; }

        [RegularExpression("Essay|MultipleChoice", ErrorMessage = "QuestionType must be Essay or MultipleChoice")]
        public string? QuestionType { get; set; }

        public List<QuestionOptionUpdateItem>? Options { get; set; }
    }

    public class QuestionOptionUpdateItem
    {
        [Required]
        [StringLength(10)]
        public string OptionLabel { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string OptionContent { get; set; } = string.Empty;
    }
}
