using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-questions")]
    [Authorize]
    [SwaggerTag("Exam Questions Management APIs")]

    public class ExamQuestionsController : ControllerBase
    {
        private readonly IExamQuestionService _examQuestionService;

        public ExamQuestionsController(IExamQuestionService examQuestionService)
        {
            _examQuestionService = examQuestionService;
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

        // =========================================================
        // GET /api/exam-questions/{id}
        // =========================================================
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "📝 Get question by ID",
            Description = "Retrieves a specific exam question with its options.")]
        public async Task<ActionResult<ResponseDto>> GetQuestion(Guid id)
        {
            var result = await _examQuestionService.GetQuestionById(id);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // PUT /api/exam-questions/{id}
        // =========================================================
        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "📝 Update question",
            Description = "Updates an existing exam question. Requires ownership of the exam paper.")]
        public async Task<ActionResult<ResponseDto>> UpdateQuestion(Guid id, [FromBody] UpdateExamQuestionDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examQuestionService.UpdateQuestion(id, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // DELETE /api/exam-questions/{id}
        // =========================================================
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "📝 Delete question",
            Description = "Deletes an exam question. Requires ownership of the exam paper.")]
        public async Task<ActionResult<ResponseDto>> DeleteQuestion(Guid id)
        {
            var result = await _examQuestionService.DeleteQuestion(id, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
