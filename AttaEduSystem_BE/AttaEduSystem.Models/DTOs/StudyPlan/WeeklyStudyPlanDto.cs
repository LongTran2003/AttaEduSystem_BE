namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// Lịch học tuần đầy đủ (Main response DTO)
    /// </summary>
    public class WeeklyStudyPlanDto
    {
        public Guid StudyPlanId { get; set; }
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }

        /// <summary>
        /// Tóm tắt tình hình học tập (AI generated)
        /// </summary>
        public string Summary { get; set; } = null!;

        /// <summary>
        /// Thống kê môn yếu/mạnh
        /// </summary>
        public SubjectPerformanceSummaryDto Performance { get; set; } = null!;

        /// <summary>
        /// Lịch học chi tiết 7 ngày (Monday -> Sunday)
        /// </summary>
        public List<DailyPlanDto> DailyPlans { get; set; } = new();

        /// <summary>
        /// Confidence score (0-100) của AI về plan này
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// Trạng thái: "Generated", "Saved", "Active", "Completed", "Cancelled"
        /// </summary>
        public string Status { get; set; } = "Generated";

        /// <summary>
        /// Ghi chú của user (nếu đã save)
        /// </summary>
        public string? Notes { get; set; }

        public DateTime GeneratedAt { get; set; }
        public DateTime? SavedAt { get; set; }

        /// <summary>
        /// Tiến độ hoàn thành (0-100%)
        /// </summary>
        public double CompletionPercentage { get; set; }
    }
}
