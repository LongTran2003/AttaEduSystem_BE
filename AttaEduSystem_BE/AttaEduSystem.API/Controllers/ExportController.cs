using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Export;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/export")]
    [Authorize]
    [SwaggerTag("Export APIs - PDF, Excel, etc.")]

    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        // =========================================================
        // GET /api/export/pdf/{id} - Export Exam to PDF
        // =========================================================
        [HttpGet("pdf/{id:guid}")]
        [SwaggerOperation(
            Summary = "📄 Export exam to PDF",
            Description = "Export ExamPaper/GeneratedExam (source)  to PDF format. " +
            "Can include answer key and optionally upload to cloud(broken, fix later).")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ExportPdfResponseDto), 200)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        [ProducesResponseType(typeof(ResponseDto), 404)]
        public async Task<IActionResult> ExportToPdf(
            Guid id,
            [FromQuery] string source = "ExamPaper",
            [FromQuery] bool includeAnswers = false)
            //[FromQuery] bool uploadToCloud = false)
        {
            // Validate source
            var validSources = new[] { "ExamPaper", "GeneratedExam" };
            if (!validSources.Contains(source, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Invalid source. Must be 'ExamPaper' or 'GeneratedExam'."
                });
            }

            var request = new ExportPdfRequestDto
            {
                Source = source,
                IncludeAnswers = includeAnswers,
                //UploadToCloud = uploadToCloud
            };

            // Pass User to service
            //var (pdfBytes, urlResponse, errorMessage) = 
            //    await _exportService.ExportToPdfAsync(id, request, User);

            var (pdfBytes, errorMessage) = await _exportService.ExportToPdfAsync(id, request, User);

            // Error case
            if (!string.IsNullOrEmpty(errorMessage))
            {
                var statusCode = errorMessage.Contains("not found") ? 404 : 500;
                return StatusCode(statusCode, new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = statusCode,
                    Message = errorMessage
                });
            }

            // Upload to cloud case
            //if (urlResponse != null)
            //{
            //    return Ok(new ResponseDto
            //    {
            //        IsSuccess = true,
            //        StatusCode = 200,
            //        Message = "PDF exported and uploaded successfully",
            //        Result = urlResponse
            //    });
            //}

            // Direct download case
            if (pdfBytes != null)
            {
                var fileName = $"Exam_{id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }

            return StatusCode(500, new ResponseDto
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = "Unknown error occurred during export"
            });
        }
    }
}
