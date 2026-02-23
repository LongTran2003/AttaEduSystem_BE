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

        [HttpPost("generate")]
        [SwaggerOperation(Summary = "Generate a weekly study plan (AI-driven)", 
            Description = "Generate a weekly study plan from recent exam attempts and provided preferences. " +
            "Returns a preview (not saved).")]
        public async Task<ActionResult<ResponseDto>> Generate([FromBody] GeneratePlanDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _studyPlanService.GenerateWeeklyPlanAsync(dto, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{studyPlanId:guid}/save")]
        [SwaggerOperation(Summary = "Save a generated study plan",
            Description = "Save a previously generated study plan by StudyPlanId (route). " +
            "Use query param setAsActive to make this plan active. Optional SavePlanDto may be sent in the body.")]
        public async Task<ActionResult<ResponseDto>> Save([FromRoute] Guid studyPlanId)
        {
            if (studyPlanId == Guid.Empty) return BadRequest("studyPlanId is required in route.");

            // Build minimal SavePlanDto internally (no body expected)
            var saveDto = new SavePlanDto
            {
                Notes = null,
                SetAsActive = false
            };

            var response = await _studyPlanService.SavePlanAsync(studyPlanId, saveDto, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("week")]
        [SwaggerOperation(Summary = "Get study plan by week start", 
            Description = "Provide weekStart (yyyy-MM-dd) to retrieve saved plan for that week.")]
        public async Task<ActionResult<ResponseDto>> GetByWeek([FromQuery] DateTime weekStart)
        {
            var response = await _studyPlanService.GetPlanByWeekAsync(weekStart, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("active")]
        [SwaggerOperation(Summary = "Get current active plan", 
            Description = "Retrieve the user's currently active study plan.")]
        public async Task<ActionResult<ResponseDto>> GetActive()
        {
            var response = await _studyPlanService.GetActivePlanAsync(User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("history")]
        [SwaggerOperation(Summary = "Get saved plan history", 
            Description = "Retrieve saved study plans for the user. Supports pagination.")]
        public async Task<ActionResult<ResponseDto>> GetHistory([FromQuery] int months = 3, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _studyPlanService.GetPlanHistoryAsync(User, months, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{planId:guid}")]
        [SwaggerOperation(Summary = "Delete a saved plan", 
            Description = "Soft-delete a saved study plan.")]
        public async Task<ActionResult<ResponseDto>> Delete([FromRoute] Guid planId)
        {
            var response = await _studyPlanService.DeletePlanAsync(planId, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("{planId:guid}/session")]
        [SwaggerOperation(Summary = "Update session completion status", 
            Description = "Mark a session as completed or not and optionally add notes.")]
        public async Task<ActionResult<ResponseDto>> UpdateSession([FromRoute] Guid planId, [FromBody] UpdateSessionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _studyPlanService.UpdateSessionStatusAsync(planId, dto, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{planId:guid}/regenerate (need to fix the logic)")]
        [SwaggerOperation(Summary = "Regenerate plan for remaining days", 
            Description = "Regenerate part of an existing plan (e.g., student wants a replan for remaining days).")]
        public async Task<ActionResult<ResponseDto>> Regenerate([FromRoute] Guid planId, [FromBody] RegeneratePlanDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _studyPlanService.RegeneratePlanAsync(planId, dto, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("performance")]
        [SwaggerOperation(Summary = "Get subject performance analysis", 
            Description = "Return per-subject performance used by the generator (for charts / preview).")]
        public async Task<ActionResult<ResponseDto>> GetPerformance([FromQuery] int lookbackDays = 30)
        {
            var response = await _studyPlanService.GetSubjectPerformanceAsync(User, lookbackDays);
            return StatusCode(response.StatusCode, response);
        }
    }
}
