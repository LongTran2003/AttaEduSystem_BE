using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-generating")]
    [SwaggerTag("Exam Generating APIs (Gemini AI)")]

    public class ExamGeneratingController : ControllerBase
    {
        private readonly IExamGeneratingService _examGeneratingService;

        public ExamGeneratingController(IExamGeneratingService examGeneratingService)
        {
            _examGeneratingService = examGeneratingService;
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
        
        [HttpPost("{originalExamId}/generate-similar")]
        [Authorize(Policy = "RequireProPlan")]
        [SwaggerOperation(Summary = "🤖 Generate similar exam", 
            Description = "Uses AI to generate a new exam based on the structure of an original exam.")]
        public async Task<ActionResult<ResponseDto>> GenerateSimilarExam(string originalExamId)
        {
            if (!Guid.TryParse(originalExamId, out var parsedId))
                return StatusCode(400, new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Invalid GUID format."
                });

            var result = await _examGeneratingService.GenerateSimilarExam(parsedId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "📚 List generated exams (paged)",
            Description = "Returns a paginated list of generated exams. Supports filterOn, filterQuery and sortBy.")]
        public async Task<ActionResult<ResponseDto>> GetAllGeneratedExams(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterOn = null,
            [FromQuery] string? filterQuery = null,
            [FromQuery] string? sortBy = null)
        {
            var result = await _examGeneratingService.GetAllGeneratedExams(pageNumber, pageSize, filterOn, filterQuery, sortBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "📚 Get generated exam by id",
            Description = "Retrieve a single generated exam by its id.")]
        public async Task<ActionResult<ResponseDto>> GetGeneratedExamById(Guid id)
        {
            var result = await _examGeneratingService.GetGeneratedExamById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-generated")]
        [SwaggerOperation(Summary = "📚 List my generated exams",
            Description = "Returns generated exams created by the current user.")]
        public async Task<ActionResult<ResponseDto>> GetGeneratedExamsByUser()
        {
            var result = await _examGeneratingService.GetGeneratedExamsByUser(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "🤖 Update generated exam status", 
            Description = "Update status (Draft, Saved, Deleted) for a generated exam.")]
        public async Task<ActionResult<ResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateGeneratedExamStatusDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examGeneratingService.UpdateStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
