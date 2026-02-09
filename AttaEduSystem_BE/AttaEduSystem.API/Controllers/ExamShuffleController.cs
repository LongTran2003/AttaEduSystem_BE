using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamShuffle;
using AttaEduSystem.Models.DTOs.QuestionBank;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.InteropServices;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "TEACHER, ADMIN")]
    [SwaggerTag("Exam Shuffle & Question Bank APIs")]

    public class ExamShuffleController : ControllerBase
    {
        private readonly IExamShuffleService _examShuffleService;

        public ExamShuffleController(IExamShuffleService examShuffleService)
        {
            _examShuffleService = examShuffleService;
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
        // POST /api/exam-papers/{id}/shuffle
        // =========================================================
        [HttpPost("api/exam-papers/{id:guid}/shuffle")]
        [SwaggerOperation(Summary = "🔀 Shuffle exam questions and options",
            Description = "Creates shuffled variants of an exam paper. Randomizes question order and/or answer options.")]
        public async Task<ActionResult<ResponseDto>> ShuffleExam(Guid id, [FromBody] ShuffleExamRequestDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examShuffleService.ShuffleExam(id, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/exam-papers/{id}/shuffled-variants
        // =========================================================
        [HttpGet("api/exam-papers/{id:guid}/shuffled-variants")]
        [SwaggerOperation(Summary = "🔀 Get all shuffled variants of an exam",
            Description = "Retrieves all previously generated shuffled variants of an exam paper.")]
        public async Task<ActionResult<ResponseDto>> GetShuffledVariants(Guid id)
        {
            var result = await _examShuffleService.GetShuffledVariants(id, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/question-bank
        // =========================================================
        [HttpGet("api/question-bank")]
        [SwaggerOperation(Summary = "📚 Search question bank",
            Description = "Search and filter questions from your exams or public exams. Use 'Scope' to filter: 'My', 'Public', or 'All'.")]
        public async Task<ActionResult<ResponseDto>> SearchQuestionBank([FromQuery] QuestionBankFilterDto filterDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examShuffleService.SearchQuestionBank(filterDto, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
