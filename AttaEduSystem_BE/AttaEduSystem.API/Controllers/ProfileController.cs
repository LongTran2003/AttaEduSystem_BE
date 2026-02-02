using AttaEduSystem.Models.DTOs;
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
    
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get current user profile", 
        Description = "Fetches detailed profile information of the logged-in user.")]
    public async Task<ActionResult<ResponseDto>> GetUserProfile()
    {
        var responseDto = await _profileService.GetUserProfile(User);
        return StatusCode(responseDto.StatusCode, responseDto);
    }

    [HttpPut("update")]
    [SwaggerOperation(Summary = "Update user profile", 
        Description = "Updates personal information (Full Name, Address, Phone, etc.) of the logged-in user.")]
    public async Task<ActionResult<ResponseDto>> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
    {
        if (!ModelState.IsValid) return ReturnInvalidInputResponse();

        var responseDto = await _profileService.UpdateUserProfile(User, updateUserProfileDto);
        return StatusCode(responseDto.StatusCode, responseDto);
    }

    [HttpPost("token/refresh")]
    [AllowAnonymous] // Thường refresh token không cần Authorize bearer cũ (vì nó hết hạn rồi)
    [SwaggerOperation(Summary = "Refresh access token", 
        Description = "Generates a new access token using a valid refresh token.")]
    public async Task<ActionResult<ResponseDto>> RefreshAccessToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid) return ReturnInvalidInputResponse();

        var responseDto = await _profileService.RefreshAccessToken(refreshTokenDto);
        return StatusCode(responseDto.StatusCode, responseDto);
    }
}