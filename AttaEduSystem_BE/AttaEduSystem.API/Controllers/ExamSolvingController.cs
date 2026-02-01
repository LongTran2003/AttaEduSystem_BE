using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-solving")]
    [SwaggerTag("Exam Solving APIs (Gemini AI)")]

    public class ExamSolvingController : ControllerBase
    {
        private readonly IExamSolvingService _examSolvingService;

        public ExamSolvingController(IExamSolvingService examSolvingService)
        {
            _examSolvingService = examSolvingService;
        }

        /// <summary>
        /// Trigger AI to solve a specific exam paper and save the solution.
        /// </summary>
        /// <param name="examPaperId">The ID of the exam paper to solve.</param>
        [Authorize(Policy = "RequireProPlan")]
        [HttpPost("{examPaperId:guid}/solve")]
        [SwaggerOperation(
            Summary = "Solve an exam paper",
            Description = "Uses Gemini AI to generate step-by-step solutions for the scanned exam paper.")]
        public async Task<ActionResult<ResponseDto>> SolveExam(Guid examPaperId)
        {
            var result = await _examSolvingService.SolveExamPaper(examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve the existing solution for an exam paper.
        /// </summary>
        /// <param name="examPaperId">The ID of the exam paper.</param>
        [HttpGet("{examPaperId:guid}")]
        [SwaggerOperation(
            Summary = "Get exam solution",
            Description = "Retrieves the saved solution for a specific exam paper.")]
        public async Task<ActionResult<ResponseDto>> GetSolution(Guid examPaperId)
        {
            var result = await _examSolvingService.GetSolutionByExamId(examPaperId);
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "Update solution status", Description = "Update status (Saved, Deleted).")]
        public async Task<ActionResult<ResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateSolutionStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _examSolvingService.UpdateStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
