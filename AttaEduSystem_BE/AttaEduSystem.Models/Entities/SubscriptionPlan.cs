using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.Entities
{
    public class SubscriptionPlan : BaseEntity<string, string, string>
    {
            [Key]
            public Guid SubscriptionPlanId { get; set; }

            [Required]
            [StringLength(50)]
            public string Code { get; set; } = null!; // "FREE", "BASIC", "PRO"

            [Required]
            [StringLength(100)]
            public string Name { get; set; } = null!; // "Free plan", "Pro Monthly"

            [StringLength(250)]
            public string? Description { get; set; }

            [Range(0, double.MaxValue)]
            public decimal PricePerMonth { get; set; } // VND / tháng

            // Giới hạn sử dụng (tùy bạn có dùng hết hay không)
            public int MaxTokensPerMonth { get; set; }
            public int MaxScansPerMonth { get; set; }
            public int MaxGeneratedExamsPerMonth { get; set; }
            public int MaxSolvesPerMonth { get; set; }

            // Bật/tắt các tính năng nâng cao (có thể parse JSON ở tầng service)
            public string? FeaturesJson { get; set; }

    }
}
