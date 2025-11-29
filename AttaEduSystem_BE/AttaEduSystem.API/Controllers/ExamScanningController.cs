using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-scanning")]
    [SwaggerTag("Exam Scanning Management APIs (Gemini AI)")]

    public class ExamScanningController : ControllerBase
    {

        private readonly IExamScanningService _examScanningService;
        

        public ExamScanningController(IExamScanningService examScanningService)
        {
            _examScanningService = examScanningService;
        }

        /// <summary>
        /// Uploads an exam paper image and performs OCR scanning.
        /// </summary>
        /// <param name="dto">Upload payload with image and metadata.</param>
        [HttpPost("scan")]
        [SwaggerOperation(
            Summary = "Scan exam paper",
            Description = "Uploads an exam paper image, stores it, and performs OCR to extract text")]
        public async Task<ActionResult<ResponseDto>> ScanExamPaper([FromForm] UploadExamPaperDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid input data.",
                    Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _examScanningService.ScanExamPaper(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets full exam paper information by ID.
        /// </summary>
        /// <param name="id">Exam paper ID.</param>
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get exam paper details",
            Description = "Returns the stored exam paper metadata and OCR text")]
        public async Task<ActionResult<ResponseDto>> GetExamPaper(Guid id)
        {
            var result = await _examScanningService.GetExamPaperById(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Returns all scanned exam papers that are marked as Ready (paginated).
        /// </summary>
        [HttpGet("ready")]
        [AllowAnonymous] // hoặc [Authorize] tùy yêu cầu
        [SwaggerOperation(
            Summary = "List get all ready exam papers",
            Description = "Provides a paginated list of exam papers whose status is Ready")]
        public async Task<ActionResult<ResponseDto>> GetReadyExamPapers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterOn = null,
            [FromQuery] string? filterQuery = null,
            [FromQuery] string? sortBy = null)
        {
            var result = await _examScanningService.GetAllReadyExamPapers(pageNumber, pageSize, filterOn, filterQuery, sortBy);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets only the OCR text of the specified exam paper.
        /// </summary>
        /// <param name="id">Exam paper ID.</param>
        [HttpGet("{id:guid}/scanned-text")]
        [SwaggerOperation(
            Summary = "Get scanned text",
            Description = "Returns the OCR text extracted from the exam paper image")]
        public async Task<ActionResult<ResponseDto>> GetScannedText(Guid id)
        {
            var result = await _examScanningService.GetScannedText(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lists all exam papers uploaded by the current user.
        /// </summary>
        [HttpGet("my-exams")]
        [SwaggerOperation(
            Summary = "List current user's exam papers",
            Description = "Returns all exam papers that the authenticated user uploaded")]
        public async Task<ActionResult<ResponseDto>> GetMyExamPapers()
        {
            var result = await _examScanningService.GetExamPapersByUser(User);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get the Cloudinary URL of the exam paper image by ID.
        /// </summary>
        [HttpGet("{id:guid}/image")]
        [SwaggerOperation(Summary = "Get exam paper image URL", Description = "Returns the stored Cloudinary URL of the exam paper image")]
        public async Task<ActionResult<ResponseDto>> GetExamPaperImage(Guid id)
        {
            var result = await _examScanningService.GetExamPaperImage(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the status of an exam paper (e.g., confirmed, removed).
        /// </summary>
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "Update exam paper status", Description = "Allows teachers to confirm or remove scanned exam papers")]
        public async Task<ActionResult<ResponseDto>> UpdateExamPaperStatus(Guid id, [FromBody] UpdateExamPaperStatusDto dto)
        {
            var result = await _examScanningService.UpdateExamPaperStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
