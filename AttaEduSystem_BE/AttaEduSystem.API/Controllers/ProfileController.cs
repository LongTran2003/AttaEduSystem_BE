using AttaEduSystem.Models.DTOs.Profile;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers;

[ApiController]
[Route("api/profiles")] // Đổi route cho đúng ngữ nghĩa
[Authorize] // Bắt buộc đăng nhập cho tất cả API trong này
[SwaggerTag("User Profile Management APIs")]

public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Get current user profile",
        Description = "Fetches detailed profile information of the logged-in user.")]
    public async Task<IActionResult> GetUserProfile()
    {
        var responseDto = await _profileService.GetUserProfile(User);
        return StatusCode(responseDto.StatusCode, responseDto);
    }

    [HttpPut("update")]
    [SwaggerOperation(
        Summary = "Update user profile",
        Description = "Updates personal information (Full Name, Address, Phone, etc.) of the logged-in user.")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
    {
        var responseDto = await _profileService.UpdateUserProfile(User, updateUserProfileDto);
        return StatusCode(responseDto.StatusCode, responseDto);
    }
}