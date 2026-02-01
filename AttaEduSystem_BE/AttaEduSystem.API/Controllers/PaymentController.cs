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
    [SwaggerTag("Payment & subscription billing APIs (PayOS)")]

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

        /// <summary>
        /// Create a PayOS payment link for a given order.
        /// </summary>
        [HttpPost("payos/create-link")]
        [SwaggerOperation(
            Summary = "Create PayOS payment link",
            Description = "Creates a PayOS payment link for the specified orderNumber.")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDto>> CreatePayOsPaymentLink(
            [FromBody] CreatePaymentLinkDto createPaymentLinkDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid input data.",
                    Result = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _paymentService.CreatePayOsPaymentLink(User, createPaymentLinkDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Confirm PayOS transaction status for an order (after redirect / callback).
        /// </summary>
        [HttpPost("payos/confirm")]
        [AllowAnonymous] // hoặc giữ [Authorize] nếu bạn confirm qua FE thay vì callback trực tiếp
        [SwaggerOperation(
            Summary = "Confirm PayOS transaction",
            Description = "Confirms payment status from PayOS for a given orderNumber.")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDto>> ConfirmPayOsTransaction(
            [FromBody] ConfirmPaymentDto confirmPaymentDto)
        {
            // 1. Gọi Service để update trạng thái Payment/Order
            var result = await _paymentService.ConfirmPayOsTransaction(confirmPaymentDto);

            // 2. Nếu thành công và trạng thái là PAID -> Kích hoạt gói
            if (result.IsSuccess && result.Result != null)
            {
                try 
                {
                    // 1. Ép kiểu sang Dictionary (An toàn hơn dynamic)
                    var data = result.Result as Dictionary<string, object>;
        
                    // 2. Lấy status (Check null cho chắc)
                    if (data != null && data.ContainsKey("payOsStatus"))
                    {
                        string payOsStatus = data["payOsStatus"]?.ToString();

                        if (payOsStatus == "PAID")
                        {
                            // 3. Lấy OrderId
                            if (data.ContainsKey("orderId") && Guid.TryParse(data["orderId"]?.ToString(), out Guid orderId))
                            {
                                // 4. Kích hoạt
                                await _subscriptionService.ActivateFromOrder(orderId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // In lỗi ra console để debug nếu có sự cố
                    Console.WriteLine("Error triggering activation: " + ex.Message);
                }
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
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
        public async Task<ActionResult<ResponseDto>> GetById(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentById(User, paymentId);
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpGet("payos/callback")]
        public async Task<IActionResult> PayOsCallback(
                [FromQuery(Name = "orderCode")] long orderCode,
                [FromQuery] string status)
        {
            var confirmDto = new ConfirmPaymentDto { OrderNumber = orderCode };

            // 1. Gọi Service update trạng thái
            var result = await _paymentService.ConfirmPayOsTransaction(confirmDto);

            // 2. Kích hoạt gói nếu thanh toán thành công (Logic tương tự bên trên)
            if (result.IsSuccess && result.Result != null)
            {
                try 
                {
                    // 1. Ép kiểu sang Dictionary (An toàn hơn dynamic)
                    var data = result.Result as Dictionary<string, object>;
        
                    // 2. Lấy status (Check null cho chắc)
                    if (data != null && data.ContainsKey("payOsStatus"))
                    {
                        string payOsStatus = data["payOsStatus"]?.ToString();

                        if (payOsStatus == "PAID")
                        {
                            // 3. Lấy OrderId
                            if (data.ContainsKey("orderId") && Guid.TryParse(data["orderId"]?.ToString(), out Guid orderId))
                            {
                                // 4. Kích hoạt
                                await _subscriptionService.ActivateFromOrder(orderId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // In lỗi ra console để debug nếu có sự cố
                    Console.WriteLine("Error triggering activation: " + ex.Message);
                }
            }

            // 3. Redirect (Giữ nguyên)
            var frontendBaseUrl = _configuration["Frontend:PaymentResultUrl"]
                            ?? "https://your-frontend-url.com/payment-result";
            var redirectUrl = $"{frontendBaseUrl}?orderCode={orderCode}&status={(result.IsSuccess ? "success" : "fail")}";

            return Redirect(redirectUrl);
        }
    }
}
