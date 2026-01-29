namespace AttaEduSystem.Models.DTOs.Billing
{
    public class GetUserSubscriptionDto
    {
        public Guid UserSubscriptionId { get; set; }
        public GetSubscriptionPlanDto Plan { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public bool IsAutoRenew { get; set; }
    }
