using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/billing")]
    [Authorize]
    [SwaggerTag("Billing Management APIs")]
    public class BillingController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUsageTrackerService _usageTracker;

        [HttpGet("plans")]
        public async Task<ActionResult<ResponseDto>> GetAvailablePlans()
        {
            var result = await _subscriptionService.GetAvailablePlans();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("subscription")]
        public async Task<ActionResult<ResponseDto>> GetCurrentSubscription()
        {
            var result = await _subscriptionService.GetCurrentSubscription(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("usage")]
        public async Task<ActionResult<ResponseDto>> GetUsage()
        {
            var result = await _usageTracker.GetUsageInfo(User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
