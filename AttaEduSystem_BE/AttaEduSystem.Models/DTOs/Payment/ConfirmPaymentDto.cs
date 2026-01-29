namespace AttaEduSystem.Models.DTOs.Payment
{
    public class ConfirmPaymentDto
    {
        // chỉ cần orderCode là đủ để xác định đơn
        public long OrderNumber { get; set; }
        // nếu sau này cần, có thể thêm PaymentTransactionId
        // public Guid PaymentTransactionId { get; set; }
    }
}
