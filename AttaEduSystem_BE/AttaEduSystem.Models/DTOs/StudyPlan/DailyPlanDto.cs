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
        /// Danh sách buổi học trong ngày
        /// </summary>
        public List<StudySessionDto> Sessions { get; set; } = new();

        /// <summary>
        /// Tổng giờ học trong ngày
        /// </summary>
        public double TotalHours { get; set; }
    }
}
