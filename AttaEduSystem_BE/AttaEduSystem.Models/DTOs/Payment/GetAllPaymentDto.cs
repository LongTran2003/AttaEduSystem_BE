namespace AttaEduSystem.Models.DTOs.Payment
{
    public class GetAllPaymentDto
    {
        public Guid PaymentTransactionId { get; set; }
        public long? OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
