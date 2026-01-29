using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Billing
{
    public class CreateCheckoutRequestDto
    {
        [Required]
        public Guid SubscriptionPlanId { get; set; }

        [Required]
        [Url]
        public string CancelUrl { get; set; } = null!;

        [Required]
        [Url]
        public string ReturnUrl { get; set; } = null!;
    }
}
