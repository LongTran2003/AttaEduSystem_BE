using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Admin;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = StaticUserRoles.Admin)]
    [SwaggerTag("Admin User Management APIs")]

    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        [SwaggerOperation(Summary = "📋 Get user list (Admin)",
            Description = "Get all users with pagination, sorting, and filtering options.")]
        public async Task<ActionResult<ResponseDto>> GetAllUsers([FromQuery] UserFilterDto filterDto)
        {
            var response = await _adminService.GetAllUsers(filterDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("users/{id}")]
        [SwaggerOperation(Summary = "🔍 Get user details",
            Description = "Get detailed information of a specific user by ID.")]
        public async Task<ActionResult<ResponseDto>> GetUserById(string id)
        {
            var response = await _adminService.GetUserById(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("users/{id}")]
        [SwaggerOperation(Summary = "✏️ Update user info",
            Description = "Update specific user's information.")]
        public async Task<ActionResult<ResponseDto>> UpdateUser(string id, [FromBody] UpdateUserDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Invalid input data",
                    Result = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)
                });
            }

            var response = await _adminService.UpdateUser(id, updateDto, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("users/{id}")]
        [SwaggerOperation(Summary = "🗑️ Soft delete user",
            Description = "Soft delete a user by changing status to 'Deleted'. Admin cannot delete themselves.")]
        public async Task<ActionResult<ResponseDto>> DeleteUser(string id)
        {
            var response = await _adminService.DeleteUser(id, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("users/{id}/lock")]
        [SwaggerOperation(Summary = "🔒 Lock user account",
            Description = "Lock a user account for a specific duration (in days).")]
        public async Task<ActionResult<ResponseDto>> LockUser(string id, [FromQuery] int days = 7)
        {
            if (days <= 0)
                return BadRequest(new ResponseDto { IsSuccess = false, StatusCode = 400, Message = "Lock duration must be greater than 0." });

            var response = await _adminService.LockUser(id, days, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("users/{id}/unlock")]
        [SwaggerOperation(Summary = "🔓 Unlock user account",
            Description = "Unlock a locked user account immediately.")]
        public async Task<ActionResult<ResponseDto>> UnlockUser(string id)
        {
            var response = await _adminService.UnlockUser(id, User);
            return StatusCode(response.StatusCode, response);
        }
    }
}
