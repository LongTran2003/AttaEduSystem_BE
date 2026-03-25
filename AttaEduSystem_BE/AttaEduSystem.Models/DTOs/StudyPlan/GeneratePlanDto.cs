using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// DTO để tạo lịch học tuần từ AI
    /// </summary>
    public class GeneratePlanDto
    {
        /// <summary>
        /// Ngày bắt đầu tuần (Monday), default = tuần kế tiếp
        /// </summary>
        public DateTime? WeekStart { get; set; }

        /// <summary>
        /// Số ngày nhìn lại kết quả học tập (default = 30)
        /// </summary>
        [Range(7, 90, ErrorMessage = "Lookback days must be between 7-90")]
        public int LookbackDays { get; set; } = 30;

        /// <summary>
        /// Số giờ học mỗi ngày (ví dụ: 2.5h)
        /// </summary>
        [Range(0.5, 10, ErrorMessage = "Daily study hours must be 0.5-10")]
        public double? DailyStudyHours { get; set; } = 2.0;

        /// <summary>
        /// Số buổi học tối đa mỗi ngày (default = 2)
        /// </summary>
        [Range(1, 5, ErrorMessage = "Max sessions per day must be 1-5")]
        public int? MaxSessionsPerDay { get; set; } = 2;

        /// <summary>
        /// Danh sách môn ưu tiên (optional, null = AI tự chọn)
        /// </summary>
        public List<string>? FocusSubjects { get; set; }

        /// <summary>
        /// Có bao gồm môn đã học tốt không? (default = false, chỉ focus yếu)
        /// </summary>
        public bool? IncludeStrongSubjects { get; set; } = false;

        /// <summary>
        /// Nếu true: generate xong sẽ lưu ngay + set làm active plan. Default = false (chỉ preview).
        /// </summary>
        public bool SaveImmediately { get; set; } = false;

        /// <summary>
        /// Ghi chú kèm theo khi lưu (chỉ dùng khi SaveImmediately = true)
        /// </summary>
        public string? Notes { get; set; }
    }
}
