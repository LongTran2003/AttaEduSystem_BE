using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AttaEduSystem.Models.Entities
{
    /// <summary>
    /// Lưu trữ lịch học tuần của user
    /// </summary>
    public class StudyPlan : BaseEntity<string, string, string>
    {
        [Key]
        public Guid StudyPlanId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Ngày bắt đầu tuần (Monday)
        /// </summary>
        [Required]
        public DateTime WeekStart { get; set; }

        /// <summary>
        /// Ngày kết thúc tuần (Sunday)
        /// </summary>
        [Required]
        public DateTime WeekEnd { get; set; }

        /// <summary>
        /// Lưu toàn bộ plan dạng JSON (WeeklyStudyPlanDto)
        /// </summary>
        [Required]
        public string PlanJson { get; set; } = null!;

        /// <summary>
        /// Ghi chú của user
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Tiến độ hoàn thành (0-100)
        /// </summary>
        public double CompletionPercentage { get; set; } = 0;

        public StudyPlan()
        {
            Status = "Active";
        }
    }
}
