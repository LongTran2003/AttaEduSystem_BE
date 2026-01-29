using AttaEduSystem.Models.DTOs;
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

        /// <summary>
        /// Generate a new similar exam based on an original exam paper's structure.
        /// </summary>
        /// <param name="originalExamId">The ID of the scanned exam paper to use as a template.</param>
        [Authorize(Policy = "RequireProPlan")]
        [HttpPost("{originalExamId:guid}/generate-similar")]
        [SwaggerOperation(
            Summary = "Generate similar exam",
            Description = "Creates a new practice exam with different numbers/context but same structure as the original.")]
        public async Task<ActionResult<ResponseDto>> GenerateSimilarExam(Guid originalExamId)
        {
            var result = await _examGeneratingService.GenerateSimilarExam(originalExamId, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
