using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.SubmitExam;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers;

[ApiController]
[Route("api/exam-taking")]
[Authorize]
[SwaggerTag("Exam Taking & Grading APIs")]

public class ExamTakingController : ControllerBase
{
    private readonly IExamTakingService _examTakingService;

    public ExamTakingController(IExamTakingService examTakingService)
    {
        _examTakingService = examTakingService;
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
    
    [HttpPost("submit")]
    [SwaggerOperation(Summary = "✍️ Submit exam answers", 
        Description = "Submits user answers, calculates score, and saves the attempt.")]
    public async Task<ActionResult<ResponseDto>> SubmitExam([FromBody] SubmitExamDto dto)
    {
        if (!ModelState.IsValid) return ReturnInvalidInputResponse();

        var result = await _examTakingService.SubmitExam(dto, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history")]
    [SwaggerOperation(Summary = "✍️ Get exam history", 
        Description = "Returns a list of all exams taken by the current user.")]
    public async Task<ActionResult<ResponseDto>> GetHistory()
    {
        var result = await _examTakingService.GetExamHistory(User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("result/{attemptId:guid}")]
    [SwaggerOperation(Summary = "✍️ Get exam result details", 
        Description = "Retrieves detailed results (score, answers) of a specific attempt.")]
    public async Task<ActionResult<ResponseDto>> GetResult(Guid attemptId)
    {
        var result = await _examTakingService.GetExamResult(attemptId, User);
        return StatusCode(result.StatusCode, result);
    }

    // =========================================================
    // POST /api/exam-taking/{attemptId}/auto-submit
    // =========================================================
    [HttpPost("{attemptId:guid}/auto-submit")]
    [Authorize]
    [SwaggerOperation(Summary = "⏰ Auto-submit exam",
        Description = "Automatically submits the exam when time is up. Grades only answered questions.")]
    public async Task<ActionResult<ResponseDto>> AutoSubmitExam(Guid attemptId)
    {
        var result = await _examTakingService.AutoSubmitExam(attemptId, User);
        return StatusCode(result.StatusCode, result);
    }

}
