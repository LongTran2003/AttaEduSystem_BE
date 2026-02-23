using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// DTO để regenerate lịch học giữa tuần
    /// </summary>
    public class RegeneratePlanDto : GeneratePlanDto
    {
        /// <summary>
        /// Bắt đầu từ ngày nào (default = hôm nay)
        /// </summary>
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Số ngày còn lại cần plan (default = đến hết tuần)
        /// </summary>
        [Range(1, 7, ErrorMessage = "Remaining days must be 1-7")]
        public int RemainingDays { get; set; } = 7;
    }
}
