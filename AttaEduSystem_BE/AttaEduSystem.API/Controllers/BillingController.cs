using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/billing")]
    [Authorize]
    [SwaggerTag("Subscription & billing APIs (plans, subscription, usage, checkout)")]
    public class BillingController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUsageTrackerService _usageTracker;

        public BillingController(
            ISubscriptionService subscriptionService,
            IUsageTrackerService usageTracker)
        {
            _subscriptionService = subscriptionService;
            _usageTracker = usageTracker;
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
        
        [HttpGet("plans")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "💎 List subscription plans", 
            Description = "Returns all active subscription plans available for purchase.")]
        public async Task<ActionResult<ResponseDto>> GetPlans()
        {
            var result = await _subscriptionService.GetAvailablePlans();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("subscription")]
        [SwaggerOperation(Summary = "💎 Get current subscription", 
            Description = "Returns the active subscription details of the current user.")]
        public async Task<ActionResult<ResponseDto>> GetCurrentSubscription()
        {
            var result = await _subscriptionService.GetCurrentSubscription(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("usage")]
        [SwaggerOperation(Summary = "💎 Get usage statistics", 
            Description = "Returns current usage (scans, solves, etc.) vs limits of the plan.")]
        public async Task<ActionResult<ResponseDto>> GetUsage()
        {
            var result = await _usageTracker.GetUsageInfo(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("checkout")]
        [SwaggerOperation(Summary = "💎 Create checkout session", 
            Description = "Creates a pending order and returns PayOS payment link.")]
        public async Task<ActionResult<ResponseDto>> CreateCheckout([FromBody] CreateCheckoutRequestDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _subscriptionService.CreateCheckout(User, dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
