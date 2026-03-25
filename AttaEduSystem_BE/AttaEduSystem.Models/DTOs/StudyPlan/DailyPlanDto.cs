namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// Lịch học chi tiết của 1 ngày
    /// </summary>
    public class DailyPlanDto
    {
        public string DayOfWeek { get; set; } = null!; // "Monday", "Tuesday"...
        public DateTime Date { get; set; }

        /// <summary>
        /// Nhãn ngày theo định dạng Việt Nam: T2, T3, T4, T5, T6, T7, CN
        /// </summary>
        public string DayLabel { get; set; } = null!;

        /// <summary>
        /// Danh sách buổi học trong ngày
        /// </summary>
        public List<StudySessionDto> Sessions { get; set; } = new();

        /// <summary>
        /// Tổng giờ học trong ngày
        /// </summary>
        public double TotalHours { get; set; }
    }
}
