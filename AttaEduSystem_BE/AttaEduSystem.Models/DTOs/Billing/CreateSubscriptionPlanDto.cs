using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Billing
{
    public class CreateSubscriptionPlanDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(1, 3650)]
        public int DurationInDays { get; set; } = 30; // Thêm số ngày

        public int MaxScansPerMonth { get; set; } = 0;
        public int MaxGeneratedExamsPerMonth { get; set; } = 0;
        public int MaxSolvesPerMonth { get; set; } = 0;

    }
}
