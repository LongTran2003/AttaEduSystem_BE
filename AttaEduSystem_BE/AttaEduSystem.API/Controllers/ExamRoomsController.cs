using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamRoom;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/exam-rooms")]
    [SwaggerTag("Virtual Exam Room Management APIs")]

    public class ExamRoomsController : ControllerBase
    {
        private readonly IExamRoomService _examRoomService;
        private readonly IQrCodeService _qrCodeService;

        public ExamRoomsController(IExamRoomService examRoomService, IQrCodeService qrCodeService)
        {
            _examRoomService = examRoomService;
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

        // =========================================================
        // POST /api/exam-rooms - Tạo phòng thi (Teacher/Admin)
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "TEACHER, ADMIN")]
        [SwaggerOperation(Summary = "🏠 Create exam room",
            Description = "Creates a new virtual exam room. Requires TEACHER or ADMIN role.")]
        public async Task<ActionResult<ResponseDto>> CreateRoom([FromBody] CreateExamRoomDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _examRoomService.CreateRoom(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/exam-rooms/my-rooms - Lấy danh sách phòng của mình
        // =========================================================
        [HttpGet("my-rooms")]
        [Authorize(Roles = "TEACHER, ADMIN")]
        [SwaggerOperation(Summary = "🏠 Get my rooms",
            Description = "Retrieves all exam rooms created by the current user.")]
        public async Task<ActionResult<ResponseDto>> GetMyRooms()
        {
            var result = await _examRoomService.GetMyRooms(User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // DELETE /api/exam-rooms/{id}/cancel - Hủy phòng thi
        // =========================================================
        [HttpDelete("{id:guid}/cancel")]
        [Authorize(Roles = "TEACHER, ADMIN")]
        [SwaggerOperation(Summary = "🏠 Cancel exam room",
            Description = "Cancels an exam room. Cannot cancel finished rooms.")]
        public async Task<ActionResult<ResponseDto>> CancelRoom(Guid id)
        {
            var result = await _examRoomService.CancelRoom(id, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/exam-rooms/{code} - Lấy thông tin phòng (Public)
        // =========================================================
        [HttpGet("{code}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "🏠 Get room by code (Public)",
            Description = "Retrieves exam room information by room code. Public endpoint.")]
        public async Task<ActionResult<ResponseDto>> GetRoomByCode(string code)
        {
            var result = await _examRoomService.GetRoomByCode(code);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/exam-rooms/{code}/status - Kiểm tra trạng thái phòng
        // =========================================================
        [HttpGet("{code}/status")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "🏠 Get room status (Public)",
            Description = "Checks current status of an exam room including remaining time.")]
        public async Task<ActionResult<ResponseDto>> GetRoomStatus(string code)
        {
            var result = await _examRoomService.GetRoomStatus(code);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // POST /api/exam-rooms/{code}/join - Join phòng thi
        // =========================================================
        [HttpPost("{code}/join")]
        [Authorize]
        [SwaggerOperation(Summary = "🏠 Join exam room",
            Description = "Joins an exam room. Requires authentication.")]
        public async Task<ActionResult<ResponseDto>> JoinRoom(string code)
        {
            var result = await _examRoomService.JoinRoom(code, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/exam-rooms/{code}/qrcode - QR Code để join phòng
        // =========================================================
        [HttpGet("{code}/qrcode")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "📱 Get QR Code for room",
            Description = "Returns a QR code image (PNG) that links to the room join page.")]
        [Produces("image/png")]
        public async Task<IActionResult> GetRoomQrCode(string code)
        {
            // Verify room exists
            var result = await _examRoomService.GetRoomByCode(code);
            if (!result.IsSuccess)
                return NotFound(new { message = "Room not found" });

            var joinUrl = _qrCodeService.GetExamRoomJoinUrl(code.ToUpper());
            var qrCodeBytes = _qrCodeService.GenerateQrCode(joinUrl);

            return File(qrCodeBytes, "image/png", $"room-{code}-qr.png");
        }
    }
}
