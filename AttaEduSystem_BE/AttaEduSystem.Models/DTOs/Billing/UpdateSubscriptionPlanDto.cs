using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Billing
{
    public class UpdateSubscriptionPlanDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; } // Sửa từ PricePerMonth -> Price

        [Range(1, 3650)]
        public int? DurationInDays { get; set; } // Thêm số ngày

        public int? MaxScansPerMonth { get; set; }
        public int? MaxGeneratedExamsPerMonth { get; set; }
        public int? MaxSolvesPerMonth { get; set; }

        [StringLength(10)]
        public string? Status { get; set; }
    }
}
