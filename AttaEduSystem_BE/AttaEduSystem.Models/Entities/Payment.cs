using AttaEduSystem.Models.Enums;
using AttaEduSystem.Utilities.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class Payment : BaseEntity<string, string, string>
    {
        [Key]
        public Guid PaymentTransactionId { get; set; }

        public long OrderNumber { get; set; }

        [ForeignKey("OrderNumber")] public virtual Order? Order { get; set; }

        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? CancelUrl { get; set; }

        [StringLength(500)]
        public string? ReturnUrl { get; set; }

        public DateTime CreatedAt { get; set; } = StaticOperationStatus.Timezone.Vietnam;

        [Column(TypeName = "varchar(20)")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        //// PayOS fields
        //[StringLength(200)]
        //public string? PayOsPaymentLinkId { get; set; }

        //[StringLength(200)]
        //public string? PayOsTransactionId { get; set; }

        //public string? PayOsRawResponse { get; set; }
    }
}
