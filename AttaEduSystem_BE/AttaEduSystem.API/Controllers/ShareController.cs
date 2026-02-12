using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.SharedExam;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api")]
    [SwaggerTag("Share Exam APIs")]

    public class ShareController : ControllerBase
    {
        private readonly IShareService _shareService;

        public ShareController(IShareService shareService)
        {
            _shareService = shareService;
        }

        // =========================================================
        // POST /api/share - Create share link
        // =========================================================
        [HttpPost("share")]
        [Authorize]
        [SwaggerOperation(
            Summary = "🔗 Create share link",
            Description = "Create a shareable link for an exam. Can set expiration, password, and max views.")]
        public async Task<ActionResult<ResponseDto>> CreateShareLink([FromBody] CreateShareLinkDto dto)
        {
            var response = await _shareService.CreateShareLink(dto, User);
            return StatusCode(response.StatusCode, response);
        }

        // =========================================================
        // GET /api/shared/{token} - View shared exam (Public)
        // =========================================================
        [HttpGet("shared/{token}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "👁️ View shared exam (Public)",
            Description = "View an exam via share link. Password required if set.")]
        public async Task<ActionResult<ResponseDto>> GetSharedExam(
            string token,
            [FromQuery] string? password = null)
        {
            var response = await _shareService.GetSharedExam(token, password);
            return StatusCode(response.StatusCode, response);
        }

        // =========================================================
        // GET /api/share/my-links - Get my share links
        // =========================================================
        [HttpGet("share/my-links")]
        [Authorize]
        [SwaggerOperation(
            Summary = "📋 Get my share links",
            Description = "Get all share links created by the current user.")]
        public async Task<ActionResult<ResponseDto>> GetMyShareLinks()
        {
            var response = await _shareService.GetMyShareLinks(User);
            return StatusCode(response.StatusCode, response);
        }

        // =========================================================
        // DELETE /api/share/{id} - Deactivate share link
        // =========================================================
        [HttpDelete("share/{id:guid}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "🗑️ Deactivate share link",
            Description = "Deactivate a share link. The link will no longer be accessible.")]
        public async Task<ActionResult<ResponseDto>> DeactivateShareLink(Guid id)
        {
            var response = await _shareService.DeactivateShareLink(id, User);
            return StatusCode(response.StatusCode, response);
        }
    }
}
