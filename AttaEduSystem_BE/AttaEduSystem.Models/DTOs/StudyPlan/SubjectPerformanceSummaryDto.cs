namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// Tổng hợp hiệu suất học tập theo môn
    /// </summary>
    public class SubjectPerformanceSummaryDto
    {
        /// <summary>
        /// Danh sách môn yếu (cần ưu tiên)
        /// </summary>
        public List<SubjectStatDto> WeakSubjects { get; set; } = new();

        /// <summary>
        /// Danh sách môn mạnh (maintain)
        /// </summary>
        public List<SubjectStatDto> StrongSubjects { get; set; } = new();
    }

    /// <summary>
    /// Thống kê chi tiết 1 môn
    /// </summary>
    public class SubjectStatDto
    {
        public string Subject { get; set; } = null!;
        public double AverageScore { get; set; }
        public int AttemptCount { get; set; }
        public string Trend { get; set; } = "Stable"; // "Improving", "Declining", "Stable"
        public int Priority { get; set; } // 1-5 (1 = highest)
        public double CorrectRate { get; set; } // % trả lời đúng
    }
}
