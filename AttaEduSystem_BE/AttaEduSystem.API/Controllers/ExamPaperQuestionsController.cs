using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-papers")]
    [Authorize]
    [SwaggerTag("Exam Papers - Questions Management APIs")]

    public class ExamPaperQuestionsController : ControllerBase
    {
        private readonly IExamQuestionService _examQuestionService;

        public ExamPaperQuestionsController(IExamQuestionService examQuestionService)
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
        // GET /api/exam-papers/{id}/questions
        // =========================================================
        [HttpGet("{id:guid}/questions")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "📄 Get all questions of an exam paper",
            Description = "Retrieves all questions belonging to a specific exam paper.")]
        public async Task<ActionResult<ResponseDto>> GetQuestions(Guid id)
        {
            var result = await _examQuestionService.GetQuestionsByExamPaper(id);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // POST /api/exam-papers/{id}/questions
        // =========================================================
        [HttpPost("{id:guid}/questions")]
        [SwaggerOperation(Summary = "📄 Add question to exam paper",
            Description = "Adds a new question to an exam paper. Requires ownership.")]
        public async Task<ActionResult<ResponseDto>> AddQuestion(Guid id, [FromBody] AddExamQuestionDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examQuestionService.AddQuestionToExamPaper(id, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // PUT /api/exam-papers/{id}/reorder
        // =========================================================
        [HttpPut("{id:guid}/reorder")]
        [SwaggerOperation(Summary = "📄 Reorder questions",
            Description = "Reorders questions in an exam paper. Requires ownership.")]
        public async Task<ActionResult<ResponseDto>> ReorderQuestions(Guid id, [FromBody] ReorderQuestionsDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examQuestionService.ReorderQuestions(id, dto, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
