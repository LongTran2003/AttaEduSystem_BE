using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Billing
{
    public class CreatePaymentLinkDto
    {
        // Mã đơn hàng (orderCode) dùng cho PayOS
        public long OrderNumber { get; set; }

        // URL khi huỷ thanh toán
        public string CancelUrl { get; set; } = null!;

        // URL khi thanh toán thành công
        public string ReturnUrl { get; set; } = null!;
    }
}
