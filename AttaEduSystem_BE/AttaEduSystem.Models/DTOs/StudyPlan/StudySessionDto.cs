namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// Một buổi học cụ thể
    /// </summary>
    public class StudySessionDto
    {
        public int SessionOrder { get; set; } // 1, 2, 3...

        /// <summary>
        /// Môn học
        /// </summary>
        public string Subject { get; set; } = null!;

        /// <summary>
        /// Mục tiêu cụ thể (AI gợi ý dựa trên weak points)
        /// </summary>
        public string Goal { get; set; } = null!;

        /// <summary>
        /// Thời lượng (giờ)
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gợi ý tài nguyên (optional)
        /// </summary>
        public List<string>? SuggestedResources { get; set; }

        /// <summary>
        /// Lý do tại sao cần học (dựa trên kết quả)
        /// </summary>
        public string Reasoning { get; set; } = null!;

        /// <summary>
        /// Đã hoàn thành chưa
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Ghi chú khi hoàn thành
        /// </summary>
        public string? CompletionNotes { get; set; }

        /// <summary>
        /// Thời gian hoàn thành
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }
}
