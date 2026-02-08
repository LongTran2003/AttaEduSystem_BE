using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Models.DTOs.Payment;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    [SwaggerTag("Payment Management APIs (PayOS)")]

    public class PaymentController : ControllerBase
    {
        private readonly IPayOsService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ISubscriptionService _subscriptionService;

        public PaymentController(IPayOsService paymentService, IConfiguration configuration, ISubscriptionService subscriptionService)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _subscriptionService = subscriptionService;
        }
        
        // Helper validate
        private ActionResult<ResponseDto> ReturnInvalidInputResponse()
        {
            return StatusCode(400, new ResponseDto
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Invalid input data.",
                Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }
        
        [HttpPost("payos/create-link")]
        [SwaggerOperation(Summary = "💸 Create PayOS payment link", 
            Description = "Generates a payment link for a pending order.")]
        public async Task<ActionResult<ResponseDto>> CreatePayOsPaymentLink([FromBody] CreatePaymentLinkDto createPaymentLinkDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _paymentService.CreatePayOsPaymentLink(User, createPaymentLinkDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("payos/confirm")]
        [SwaggerOperation(Summary = "💸 Confirm PayOS transaction", 
            Description = "Updates transaction status based on PayOS callback data.")]
        public async Task<ActionResult<ResponseDto>> ConfirmPayOsTransaction([FromBody] ConfirmPaymentDto confirmPaymentDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _paymentService.ConfirmPayOsTransaction(confirmPaymentDto);

            // Logic kích hoạt gói (giữ nguyên logic cũ nhưng gọn hơn)
            if (result.IsSuccess && result.Result is Dictionary<string, object> data)
            {
                if (data.TryGetValue("payOsStatus", out var statusObj) && statusObj?.ToString() == "PAID")
                {
                    if (data.TryGetValue("orderId", out var orderIdObj) && Guid.TryParse(orderIdObj?.ToString(), out Guid orderId))
                    {
                        await _subscriptionService.ActivateFromOrder(orderId);
                    }
                }
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "💸 List all payments", 
            Description = "Returns a paginated list of payment transactions.")]
        public async Task<ActionResult<ResponseDto>> GetAll(
            int pageNumber = 1,
            int pageSize = 10,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null)
        {
            var result = await _paymentService.GetAllPayments(User, pageNumber, pageSize, filterOn, filterQuery, sortBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{paymentId:guid}")]
        [SwaggerOperation(Summary = "💸 Get payment details", 
            Description = "Retrieves details of a specific payment transaction.")]
        public async Task<ActionResult<ResponseDto>> GetById(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentById(User, paymentId);
            return StatusCode(result.StatusCode, result);
        }

        // Callback là Redirect nên không cần trả về ResponseDto
        [HttpGet("payos/callback")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)] // Có thể ẩn khỏi Swagger vì đây là callback từ PayOS
        public async Task<IActionResult> PayOsCallback([FromQuery(Name = "orderCode")] long orderCode, [FromQuery] string status)
        {
            // ... (Giữ nguyên logic redirect cũ) ...
            var confirmDto = new ConfirmPaymentDto { OrderNumber = orderCode };
            var result = await _paymentService.ConfirmPayOsTransaction(confirmDto);

            if (result.IsSuccess && result.Result is Dictionary<string, object> data)
            {
                if (data.TryGetValue("payOsStatus", out var statusObj) && statusObj?.ToString() == "PAID")
                {
                    if (data.TryGetValue("orderId", out var orderIdObj) && Guid.TryParse(orderIdObj?.ToString(), out Guid orderId))
                    {
                        await _subscriptionService.ActivateFromOrder(orderId);
                    }
                }
            }

            var frontendBaseUrl = _configuration["Frontend:PaymentResultUrl"] ?? "https://your-frontend-url.com/payment-result";
            var redirectUrl = $"{frontendBaseUrl}?orderCode={orderCode}&status={(result.IsSuccess ? "success" : "fail")}";
            return Redirect(redirectUrl);
        }
    }
}
