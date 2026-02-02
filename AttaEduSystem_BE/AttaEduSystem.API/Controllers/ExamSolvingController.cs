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
        
        [HttpPost("{examPaperId:guid}/solve")]
        [Authorize(Policy = "RequireProPlan")]
        [SwaggerOperation(Summary = "Solve an exam paper", 
            Description = "Uses AI to generate step-by-step solutions.")]
        public async Task<ActionResult<ResponseDto>> SolveExam(Guid examPaperId)
        {
            var result = await _examSolvingService.SolveExamPaper(examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{examPaperId:guid}")]
        [SwaggerOperation(Summary = "Get solution for exam", 
            Description = "Retrieves the saved AI solution for a specific exam.")]
        public async Task<ActionResult<ResponseDto>> GetSolution(Guid examPaperId)
        {
            var result = await _examSolvingService.GetSolutionByExamId(examPaperId);
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "Update solution status", 
            Description = "Update status (Saved, Deleted) for a solution.")]
        public async Task<ActionResult<ResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateSolutionStatusDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examSolvingService.UpdateStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
