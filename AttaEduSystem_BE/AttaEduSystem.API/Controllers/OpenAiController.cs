using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/open-ai")]
    [SwaggerTag("OpenAI Integration APIs (broken)")]

    public class OpenAiController : ControllerBase
    {
        private readonly IOpenAiService _openAiService;

        public OpenAiController(IOpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        [HttpPost("{id:guid}/generate")]
        [Authorize(Roles = "TEACHER, ADMIN")]
        public async Task<ActionResult<ResponseDto>> GenerateExam(Guid id, [FromBody] GenerateExamRequestDto dto)
        {
            dto.OriginalExamPaperId = id;
            var result = await _openAiService.GenerateExamAsync(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}/generated-exams")]
        [Authorize]
        public async Task<ActionResult<ResponseDto>> GetGeneratedExams(Guid id)
        {
            var result = await _openAiService.GetGeneratedExamsByOriginalAsync(id, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("generated/{generatedId:guid}")]
        [Authorize]
        public async Task<ActionResult<ResponseDto>> GetGeneratedExam(Guid generatedId)
        {
            var result = await _openAiService.GetGeneratedExamAsync(generatedId, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
