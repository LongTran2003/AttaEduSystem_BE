using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/gemini-ai")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [SwaggerTag("Gemini AI Integration APIs")]

    public class GeminiAiController : ControllerBase
    {
        private readonly IGeminiAiService _geminiAiService;

        public GeminiAiController(IGeminiAiService geminiAiService)
        {
            _geminiAiService = geminiAiService;
        }

        // 1. Upload ảnh -> Nhận về JSON cấu trúc đề
        [HttpPost("analyze-exam-structure")]
        [SwaggerOperation(Summary = "Step 1: Scan & Extract Exam Structure (JSON)", Description = "Nhập ảnh để lấy JSON, nếu muốn dùng Step 2 ,3 thì lấy JSON ở createExamPaper cho gọn")]
        public async Task<ActionResult<string>> AnalyzeExam(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return BadRequest("No image provided.");

            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());

            try
            {
                var jsonResult = await _geminiAiService.AnalyzeExamStructure(base64, imageFile.ContentType);
                return Ok(jsonResult); // Trả về JSON String
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 2. Gửi JSON đề -> Nhận về JSON lời giải
        [HttpPost("solve-exam")]
        [SwaggerOperation(Summary = "Step 2: Solve Exam based on JSON content", Description = " dùng JSON ở bước 1, copy hết dán vào")]
        public async Task<ActionResult<string>> SolveExam([FromBody] JsonElement examContent)
        {
            try
            {
                // Chuyển object JSON thành string để gửi cho AI
                var jsonString = examContent.ToString();
                var result = await _geminiAiService.SolveExam(jsonString);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 3. Gửi JSON đề cũ -> Nhận về JSON đề mới
        [HttpPost("generate-similar-exam")]
        [SwaggerOperation(Summary = "Step 3: Generate New Exam based on Format", Description = "Y chang bước 2, dùng JSON dán vào")]
        public async Task<ActionResult<string>> GenerateExam([FromBody] JsonElement originalExamFormat)
        {
            try
            {
                var jsonString = originalExamFormat.ToString();
                var result = await _geminiAiService.GenerateSimilarExam(jsonString);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
