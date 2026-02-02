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
        
        [HttpPost("{originalExamId:guid}/generate-similar")]
        [Authorize(Policy = "RequireProPlan")]
        [SwaggerOperation(Summary = "Generate similar exam", 
            Description = "Uses AI to generate a new exam based on the structure of an original exam.")]
        public async Task<ActionResult<ResponseDto>> GenerateSimilarExam(Guid originalExamId)
        {
            var result = await _examGeneratingService.GenerateSimilarExam(originalExamId, User);
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "Update generated exam status", 
            Description = "Update status (Draft, Saved, Deleted) for a generated exam.")]
        public async Task<ActionResult<ResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateGeneratedExamStatusDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examGeneratingService.UpdateStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
