using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ChatBot;
using AttaEduSystem.Models.DTOs.ChatBox;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    [SwaggerTag("Chat AI Management APIs - Conversation & History")]

    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IChatAiService _chatAiService;

        public ChatController(IChatService chatService, IChatAiService chatAiService)
        {
            _chatService = chatService;
            _chatAiService = chatAiService;
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
        // POST /api/chat/conversations - Tạo conversation mới
        // =========================================================
        [HttpPost("conversations")]
        [SwaggerOperation(
            Summary = "💬 Create new conversation",
            Description = "Creates a new chat conversation. Optionally include initial message to get AI response immediately.")]
        public async Task<ActionResult<ResponseDto>> CreateConversation([FromBody] CreateConversationDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _chatService.CreateConversationAsync(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/chat/conversations - Lấy lịch sử chat
        // =========================================================
        [HttpGet("conversations")]
        [SwaggerOperation(
            Summary = "📋 Get conversation history",
            Description = "Retrieves all chat conversations of current user with pagination.")]
        public async Task<ActionResult<ResponseDto>> GetConversations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _chatService.GetConversationsAsync(User, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // GET /api/chat/conversations/{id} - Lấy chi tiết conversation
        // =========================================================
        [HttpGet("conversations/{id:guid}")]
        [SwaggerOperation(
            Summary = "💬 Get conversation details",
            Description = "Retrieves a specific conversation with all messages.")]
        public async Task<ActionResult<ResponseDto>> GetConversation(Guid id)
        {
            var result = await _chatService.GetConversationByIdAsync(id, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // POST /api/chat/conversations/{id}/messages - Gửi message
        // =========================================================
        [HttpPost("conversations/{id:guid}/messages")]
        [SwaggerOperation(
            Summary = "✉️ Send message & get AI reply",
            Description = "Sends a message to the conversation and receives AI response.")]
        public async Task<ActionResult<ResponseDto>> SendMessage(Guid id, [FromBody] SendMessageDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _chatService.SendMessageAsync(id, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // DELETE /api/chat/conversations/{id} - Xóa conversation
        // =========================================================
        [HttpDelete("conversations/{id:guid}")]
        [SwaggerOperation(
            Summary = "🗑️ Delete conversation",
            Description = "Soft deletes a conversation (marks as deleted).")]
        public async Task<ActionResult<ResponseDto>> DeleteConversation(Guid id)
        {
            var result = await _chatService.DeleteConversationAsync(id, User);
            return StatusCode(result.StatusCode, result);
        }

        // =========================================================
        // ✅ NEW: POST /api/chat/ask-about-exam/{examId}
        // =========================================================
        [HttpPost("ask-about-exam/{examId:guid}")]
        [SwaggerOperation(
            Summary = "📚 Ask about specific exam",
            Description = "Sends a question about a specific exam paper. " +
            "AI will respond with exam context (questions, solutions if enabled). Optionally attach to existing conversation.")]
        public async Task<ActionResult<ResponseDto>> AskAboutExam(Guid examId, [FromBody] AskAboutExamDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _chatService.AskAboutExamAsync(examId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("health/ai")]
        [SwaggerOperation(
            Summary = "🩺 Check chat AI health",
            Description = "Checks chat AI configuration and optionally probes Gemini provider using runProbe=true.")]
        public async Task<ActionResult<ResponseDto>> CheckAiHealth([FromQuery] bool runProbe = false)
        {
            var (isOk, status, detail) = await _chatAiService.CheckHealthAsync(runProbe);
            var code = isOk ? 200 : 503;
            return StatusCode(code, new ResponseDto
            {
                IsSuccess = isOk,
                StatusCode = code,
                Message = isOk ? "Chat AI health check passed" : "Chat AI health check failed",
                Result = new { status, detail, runProbe }
            });
        }
    }
}
