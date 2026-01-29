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

        /// <summary>
        /// Get all available subscription plans.
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get subscription plans", Description = "Returns all active subscription plans.")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseDto>> GetPlans()
        {
            var result = await _subscriptionService.GetAvailablePlans();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get current user's active subscription.
        /// </summary>
        [HttpGet("subscription")]
        [SwaggerOperation(Summary = "Get current subscription", Description = "Returns the active subscription of current user.")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResponseDto>> GetCurrentSubscription()
        {
            var result = await _subscriptionService.GetCurrentSubscription(User);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get current usage and limits for the user.
        /// </summary>
        [HttpGet("usage")]
        [SwaggerOperation(Summary = "Get usage info", Description = "Returns current usage and limits based on subscription.")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResponseDto>> GetUsage()
        {
            var result = await _usageTracker.GetUsageInfo(User);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Create checkout session for a subscription plan (Order + PayOS link).
        /// </summary>
        [HttpPost("checkout")]
        [SwaggerOperation(
            Summary = "Create checkout session",
            Description = "Creates an order for selected subscription plan and returns PayOS checkout URL.")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDto>> CreateCheckout([FromBody] CreateCheckoutRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid input data.",
                    Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _subscriptionService.CreateCheckout(User, dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
