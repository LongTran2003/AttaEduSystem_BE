using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// DTO để lưu lịch học đã tạo
    /// </summary>
    public class SavePlanDto
    {
        /// <summary>
        /// Ghi chú riêng của user (optional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes must be less than 500 characters")]
        public string? Notes { get; set; }

        /// <summary>
        /// Có set làm plan active không (default = true)
        /// </summary>
        public bool SetAsActive { get; set; } = true;
    }
}
