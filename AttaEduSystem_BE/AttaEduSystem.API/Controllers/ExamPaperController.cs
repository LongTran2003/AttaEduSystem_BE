using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-paper")]
    [SwaggerTag("Exam Paper Management APIs")]

    public class ExamPaperController : ControllerBase
    {

        private readonly IExamScanningService _examScanningService;

        public ExamPaperController(IExamScanningService examScanningService)
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
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDto>> GetExamPaper(Guid id)
        {
            var result = await _examScanningService.GetExamPaperById(id);
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
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseDto>> GetMyExamPapers()
        {
            var result = await _examScanningService.GetExamPapersByUser(User);
            return StatusCode(result.StatusCode, result);
        }
    }
}
