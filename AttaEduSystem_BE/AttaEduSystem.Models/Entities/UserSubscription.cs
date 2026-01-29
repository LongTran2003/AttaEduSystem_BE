using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class UserSubscription : BaseEntity<string, string, string>
    {
        [Key]
        public Guid UserSubscriptionId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public Guid SubscriptionPlanId { get; set; }

        [ForeignKey(nameof(SubscriptionPlanId))]
        public SubscriptionPlan Plan { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsAutoRenew { get; set; } = false;
    }
}
