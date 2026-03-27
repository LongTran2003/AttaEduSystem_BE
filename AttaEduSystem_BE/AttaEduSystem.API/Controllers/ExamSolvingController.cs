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
        [Authorize]
        [SwaggerOperation(Summary = "💡 Solve an exam paper", 
            Description = "Uses AI to generate step-by-step solutions.")]
        public async Task<ActionResult<ResponseDto>> SolveExam(Guid examPaperId)
        {
            var result = await _examSolvingService.SolveExamPaper(examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{examPaperId:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "💡 Get solution for exam", 
            Description = "Retrieves the saved AI solution for a specific exam.")]
        public async Task<ActionResult<ResponseDto>> GetSolution(Guid examPaperId)
        {
            var result = await _examSolvingService.GetSolutionByExamId(examPaperId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("solutions")]
        [SwaggerOperation(Summary = "🧾 List exam solutions (paged)",
            Description = "Returns a paginated list of exam solutions. Supports filterOn, filterQuery and sortBy.")]
        public async Task<ActionResult<ResponseDto>> GetAllSolutions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterOn = null,
            [FromQuery] string? filterQuery = null,
            [FromQuery] string? sortBy = null)
        {
            var result = await _examSolvingService.GetAllSolutions(pageNumber, pageSize, filterOn, filterQuery, sortBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("solutions/{id:guid}")]
        [SwaggerOperation(Summary = "🧾 Get solution by id",
            Description = "Retrieve a single exam solution by its id.")]
        public async Task<ActionResult<ResponseDto>> GetSolutionById(Guid id)
        {
            var result = await _examSolvingService.GetSolutionById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-solutions")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SwaggerOperation(Summary = "🧾 List my solutions",
            Description = "Returns solutions created by the current user.")]
        public async Task<ActionResult<ResponseDto>> GetSolutionsByUser()
        {
            var result = await _examSolvingService.GetSolutionsByUser(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "💡 Update solution status", 
            Description = "Update status (Saved, Deleted) for a solution.")]
        public async Task<ActionResult<ResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateSolutionStatusDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examSolvingService.UpdateStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}/answer")]
        [Authorize]
        [SwaggerOperation(Summary = "✏️ Edit solution answer manually",
            Description = "Manually update the solution content (answers) without calling AI. " +
                          "Use this to correct AI mistakes or write custom explanations. Only owner can edit. " +
                          "Send the full solutionContentJson in the request body.")]
        public async Task<ActionResult<ResponseDto>> UpdateAnswer(Guid id, [FromBody] UpdateSolutionContentDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examSolvingService.UpdateSolutionContentAsync(id, dto.SolutionContentJson, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
