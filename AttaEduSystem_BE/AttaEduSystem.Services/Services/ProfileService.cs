using System.Security.Claims;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Profile;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace AttaEduSystem.Services.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly TokenService _tokenService;

    public ProfileService(UserManager<ApplicationUser> userManager, IMapper mapper, TokenService tokenService)
    {
        _userManager = userManager;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task<ResponseDto> GetUserProfile(ClaimsPrincipal userPrincipal)
    {
        var userId = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
        if (string.IsNullOrEmpty(userId))
        {
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);
        }

        // Lấy role
        var roles = await _userManager.GetRolesAsync(user);

        // Map data từ Database (để đảm bảo data mới nhất, không dùng data từ Token cũ)
        var userDto = new GetUserDto
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty, // Lấy từ DB
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            BirthDate = user.BirthDate,
            Address = user.Address ?? string.Empty, // Lấy từ DB
            Gender = user.Gender,
            ImageUrl = user.ImageUrl ?? string.Empty, // Lấy từ DB
            UserName = user.UserName!,
            // Logic tính tuổi
            Age = DateTime.UtcNow.Year - user.BirthDate.Year -
                  (DateTime.UtcNow.Date < user.BirthDate.AddYears(DateTime.UtcNow.Year - user.BirthDate.Year) ? 1 : 0),
            Roles = roles.ToList()
        };

        return SuccessResponse.Build("Get profile info successfully", 200, userDto);
    }

    public async Task<ResponseDto> UpdateUserProfile(ClaimsPrincipal userPrincipal,
        UpdateUserProfileDto updateUserProfileDto)
        {
            // Lấy thông tin người dùng từ token JWT
            var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new ResponseDto
                {
                    Message = "Unauthorized",
                    StatusCode = 401,
                    IsSuccess = false,
                    Result = null
                };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ResponseDto
                {
                    Message = "Invalid user",
                    StatusCode = 400,
                    IsSuccess = false,
                    Result = null
                };

            // Sử dụng mapping với overload có destination để cập nhật đối tượng user hiện có
            _mapper.Map(updateUserProfileDto, user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new ResponseDto
                {
                    Message = "Update user profile failed",
                    StatusCode = 400,
                    IsSuccess = false,
                    Result = result.Errors
                };

            // Nếu muốn trả về dữ liệu cập nhật, bạn có thể map lại đối tượng user sang DTO trả về
            var updatedUserDto = _mapper.Map<ApplicationUser, UpdateUserProfileDto>(user);
            return new ResponseDto
            {
                Message = "Update user profile successfully",
                StatusCode = 200,
                IsSuccess = true,
                Result = updatedUserDto
            };
        }
    
    public async Task<ResponseDto> RefreshAccessToken(RefreshTokenDto refreshTokenDto)
    {
        var principal = await _tokenService.GetPrincipalFromToken(refreshTokenDto.RefreshToken);
        if (principal is null)
            ErrorResponse.Build(
                StaticOperationStatus.Token.TokenInvalid,
                StaticOperationStatus.StatusCode.Unauthorized);

        var userId = principal!.FindFirstValue(ClaimTypes.NameIdentifier);
        var userFromDb = await _userManager.FindByIdAsync(userId);

        if (userFromDb is null)
            ErrorResponse.Build(
                StaticOperationStatus.User.UserNotFound,
                StaticOperationStatus.StatusCode.NotFound);

        var storedRefreshToken = await _tokenService.RetrieveRefreshTokenAsync(userId);

        if (storedRefreshToken != refreshTokenDto.RefreshToken)
            ErrorResponse.Build(
                StaticOperationStatus.Token.TokenInvalid,
                StaticOperationStatus.StatusCode.Unauthorized);
        // New access token creation
        var newAccessToken = await _tokenService.GenerateJwtAccessTokenAsync(userFromDb);

        return SuccessResponse.Build(
            StaticOperationStatus.Token.TokenRefreshed,
            StaticOperationStatus.StatusCode.Ok,
            newAccessToken);
    }
}