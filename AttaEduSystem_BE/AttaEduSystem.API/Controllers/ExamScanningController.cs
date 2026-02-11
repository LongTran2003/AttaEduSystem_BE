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
        private readonly IQrCodeService _qrCodeService;

        public ExamScanningController(IExamScanningService examScanningService, IQrCodeService qrCodeService)
        {
            _examScanningService = examScanningService;
            _qrCodeService = qrCodeService;
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

        [HttpPost("scan")]
        [SwaggerOperation(Summary = "📸 Upload & Scan exam paper", 
            Description = "Uploads an image, performs OCR, and extracts exam structure.")]
        public async Task<ActionResult<ResponseDto>> ScanExamPaper([FromForm] UploadExamPaperDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examScanningService.ScanExamPaper(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "📸 Get exam paper details with questions label", 
            Description = "Retrieves metadata and OCR content of a specific exam paper with a question label.")]
        public async Task<ActionResult<ResponseDto>> GetExamPaper(Guid id)
        {
            var result = await _examScanningService.GetExamPaperById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("ready")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "📸 List ready exam papers", 
            Description = "Get a paginated list of public exam papers marked as Ready.")]
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

        [HttpGet("{id:guid}/scanned-text")]
        [SwaggerOperation(Summary = "📸 Get scanned text only", 
            Description = "Returns the raw OCR text extracted from the exam paper.")]
        public async Task<ActionResult<ResponseDto>> GetScannedText(Guid id)
        {
            var result = await _examScanningService.GetScannedText(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-exams")]
        [SwaggerOperation(Summary = "📸 List my exam papers", 
            Description = "Returns all exam papers uploaded by the current logged-in user.")]
        public async Task<ActionResult<ResponseDto>> GetMyExamPapers()
        {
            var result = await _examScanningService.GetExamPapersByUser(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}/image")]
        [SwaggerOperation(Summary = "📸 Get exam image URL", 
            Description = "Returns the Cloudinary URL of the uploaded exam image.")]
        public async Task<ActionResult<ResponseDto>> GetExamPaperImage(Guid id)
        {
            var result = await _examScanningService.GetExamPaperImage(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(Summary = "📸 Update exam paper status", 
            Description = "Updates status (e.g., Removed) for a scanned exam paper.")]
        public async Task<ActionResult<ResponseDto>> UpdateExamPaperStatus(Guid id, [FromBody] UpdateExamPaperStatusDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examScanningService.UpdateExamPaperStatus(id, dto.Status, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/exam-papers/{id}/qrcode - QR Code để chia sẻ đề
        // =========================================================
        [HttpGet("{id:guid}/qrcode")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "📱 Get QR Code for exam paper",
            Description = "Returns a QR code image (PNG) that links to the exam practice page.")]
        [Produces("image/png")]
        public async Task<IActionResult> GetExamPaperQrCode(Guid id)
        {
            // Verify exam paper exists
            var result = await _examScanningService.GetExamPaperById(id);
            if (!result.IsSuccess)
                return NotFound(new { message = "Exam paper not found" });

            var shareUrl = _qrCodeService.GetExamPaperShareUrl(id);
            var qrCodeBytes = _qrCodeService.GenerateQrCode(shareUrl);

            return File(qrCodeBytes, "image/png", $"exam-{id}-qr.png");
        }
    }
}
