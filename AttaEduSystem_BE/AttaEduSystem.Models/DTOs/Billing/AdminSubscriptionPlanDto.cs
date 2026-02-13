namespace AttaEduSystem.Models.DTOs.Billing
{
    public class AdminSubscriptionPlanDto
    {
        public Guid SubscriptionPlanId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal PricePerMonth { get; set; }
        public int MaxScansPerMonth { get; set; }
        public int MaxGeneratedExamsPerMonth { get; set; }
        public int MaxSolvesPerMonth { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
