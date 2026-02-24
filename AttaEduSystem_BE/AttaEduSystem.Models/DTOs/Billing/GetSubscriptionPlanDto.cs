namespace AttaEduSystem.Models.DTOs.Billing
{
    public class GetSubscriptionPlanDto
    {
        public Guid SubscriptionPlanId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; } // Sửa
        public int DurationInDays { get; set; }
        public int MaxTokensPerMonth { get; set; }
        public int MaxScansPerMonth { get; set; }
        public int MaxGeneratedExamsPerMonth { get; set; }
        public int MaxSolvesPerMonth { get; set; }
        public bool IsActive { get; set; }
    }
}
