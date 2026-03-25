using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.StudyPlan;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/study-plans")]
    [Authorize]
    [SwaggerTag("Study plan / Weekly schedule APIs")]
    public class StudyPlanController : ControllerBase
    {
        private readonly IStudyPlanService _studyPlanService;

        public StudyPlanController(IStudyPlanService studyPlanService)
        {
            _studyPlanService = studyPlanService;
        }

        // Helper validate
        private ActionResult<ResponseDto> ReturnInvalidInputResponse() =>
            StatusCode(400, new ResponseDto
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Invalid input data.",
                Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        /// <summary>
        /// [1] Generate (+ optionally save) AI study plan.
        /// Set SaveImmediately=true to generate and activate in a single call.
        /// </summary>
        [HttpPost("generate")]
        [SwaggerOperation(
            Summary = "🤖 Generate AI study plan",
            Description = "Generate a weekly study plan from exam history and preferences.\n\n" +
                          "- `saveImmediately = false` (default): returns a preview, save later via `/{id}/save`.\n" +
                          "- `saveImmediately = true`: generate + save + set active in ONE call (recommended for mobile).")]
        public async Task<ActionResult<ResponseDto>> Generate([FromBody] GeneratePlanDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var response = await _studyPlanService.GenerateWeeklyPlanAsync(dto, User);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [2] Save a previously generated preview plan.
        /// Only needed when saveImmediately=false was used in /generate.
        /// </summary>
        [HttpPost("{planId:guid}/save")]
        [SwaggerOperation(
            Summary = "💾 Save a generated plan",
            Description = "Saves a generated plan (preview → active). Use `setAsActive=true` to make it the current plan. " +
                          "Skippable if you used `saveImmediately=true` in `/generate`.")]
        public async Task<ActionResult<ResponseDto>> Save([FromRoute] Guid planId, [FromBody] SavePlanDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var response = await _studyPlanService.SavePlanAsync(planId, dto, User);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [3] Get schedule — unified view endpoint.
        /// view=today  → today's sessions from active plan
        /// view=active → full active plan
        /// view=week   → plan by weekStart (requires weekStart param)
        /// view=range  → plans in date range (requires from + to params, max 31 days)
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "📅 Get schedule",
            Description = "Unified view endpoint. Use `view` param to choose mode:\n\n" +
                          "| view   | Required params | Description |\n" +
                          "|--------|-----------------|-------------|\n" +
                          "| `today`  | –               | Today's sessions from active plan |\n" +
                          "| `active` | –               | Full active plan |\n" +
                          "| `week`   | `weekStart`     | Saved plan for that week |\n" +
                          "| `range`  | `from`, `to`    | All plans in range (max 31 days) |")]
        public async Task<ActionResult<ResponseDto>> GetSchedule(
            [FromQuery] string view = "active",
            [FromQuery] DateTime? weekStart = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var response = view.ToLowerInvariant() switch
            {
                "today"  => await _studyPlanService.GetTodayPlanAsync(User),
                "active" => await _studyPlanService.GetActivePlanAsync(User),
                "week"   => weekStart.HasValue
                    ? await _studyPlanService.GetPlanByWeekAsync(weekStart.Value, User)
                    : ErrorResponse(400, "weekStart query param is required for view=week"),
                "range"  => from.HasValue && to.HasValue
                    ? await _studyPlanService.GetPlansByRangeAsync(User, from.Value, to.Value)
                    : ErrorResponse(400, "Both 'from' and 'to' query params are required for view=range"),
                _ => ErrorResponse(400, "Invalid view. Allowed: today, active, week, range")
            };
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [4] Get saved plan history with pagination.
        /// </summary>
        [HttpGet("history")]
        [SwaggerOperation(
            Summary = "📋 Get plan history",
            Description = "List saved study plans for the user. Supports pagination and lookback in months.")]
        public async Task<ActionResult<ResponseDto>> GetHistory(
            [FromQuery] int months = 3,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _studyPlanService.GetPlanHistoryAsync(User, months, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [5] Get plan detail by ID.
        /// </summary>
        [HttpGet("{planId:guid}")]
        [SwaggerOperation(Summary = "🔍 Get plan detail", Description = "Get full detail of a specific study plan by its ID.")]
        public async Task<ActionResult<ResponseDto>> GetDetail([FromRoute] Guid planId)
        {
            var response = await _studyPlanService.GetPlanDetailAsync(planId, User);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [6] Soft-delete a plan.
        /// </summary>
        [HttpPut("{planId:guid}/delete")]
        [SwaggerOperation(Summary = "🗑️ Delete a plan", Description = "Soft-delete a saved study plan (sets status to Deleted).")]
        public async Task<ActionResult<ResponseDto>> Delete([FromRoute] Guid planId)
        {
            var response = await _studyPlanService.DeletePlanAsync(planId, User);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [7] Mark a session as completed / not completed.
        /// </summary>
        [HttpPatch("{planId:guid}/session")]
        [SwaggerOperation(
            Summary = "✅ Update session completion",
            Description = "Mark a session as completed or not completed and optionally add notes.")]
        public async Task<ActionResult<ResponseDto>> UpdateSession([FromRoute] Guid planId, [FromBody] UpdateSessionDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var response = await _studyPlanService.UpdateSessionStatusAsync(planId, dto, User);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [8] Regenerate remaining days of an existing plan.
        /// </summary>
        [HttpPost("{planId:guid}/regenerate")]
        [SwaggerOperation(
            Summary = "🔄 Regenerate remaining days",
            Description = "Regenerate part of an existing plan for remaining days (e.g., student wants to replan after mid-week).")]
        public async Task<ActionResult<ResponseDto>> Regenerate([FromRoute] Guid planId, [FromBody] RegeneratePlanDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var response = await _studyPlanService.RegeneratePlanAsync(planId, dto, User);
            return StatusCode(response.StatusCode, response);
        }

        // Private helper to return inline error ResponseDto
        private static ResponseDto ErrorResponse(int statusCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
