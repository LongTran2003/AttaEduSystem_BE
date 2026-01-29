using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class Order : BaseEntity<string, string, string>
    {
        [Key]
        public Guid OrderId { get; set; }

        // PayOS orderCode
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderNumber { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public Guid SubscriptionPlanId { get; set; }

        [ForeignKey(nameof(SubscriptionPlanId))]
        public SubscriptionPlan Plan { get; set; } = null!;

        public decimal TotalPrice { get; set; }

        // 1 Order có thể có nhiều payment attempt (tạo link lại, user thanh toán lại, v.v.)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
