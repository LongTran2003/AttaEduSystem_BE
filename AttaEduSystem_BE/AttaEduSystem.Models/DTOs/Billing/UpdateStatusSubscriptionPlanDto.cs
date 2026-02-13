using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Billing
{
    public class UpdateStatusSubscriptionPlanDto
    {
        /// <summary>
        /// Target status value (e.g. "Active", "Removed")
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Status { get; set; } = null!;
    }
}
