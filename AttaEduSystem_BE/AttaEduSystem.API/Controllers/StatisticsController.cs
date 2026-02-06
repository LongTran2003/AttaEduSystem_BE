using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize]
[SwaggerTag("User Analytics & Statistics APIs")]

public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("overview")]
    [SwaggerOperation(Summary = "Get overview statistics", 
        Description = "Returns total exams, avg score, etc.")]
    public async Task<ActionResult<ResponseDto>> GetOverview()
    {
        var result = await _statisticsService.GetOverviewStats(User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("progress-chart")]
    [SwaggerOperation(Summary = "Get progress chart data", 
        Description = "Returns aggregated scores over the last 7 days (or specified days).")]
    public async Task<ActionResult<ResponseDto>> GetProgressChart([FromQuery] int days = 7)
    {
        var result = await _statisticsService.GetProgressChart(User, days);
        return StatusCode(result.StatusCode, result);
    }
}