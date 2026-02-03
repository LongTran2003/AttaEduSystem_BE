using System.Security.Claims;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Profile;
using Microsoft.AspNetCore.Http;

namespace AttaEduSystem.Services.IServices;

public interface IProfileService
{
    Task<ResponseDto> GetUserProfile(ClaimsPrincipal userPrincipal);
    Task<ResponseDto> UpdateUserProfile(ClaimsPrincipal userPrincipal, UpdateUserProfileDto updateUserProfileDto);
    Task<ResponseDto> RefreshAccessToken(RefreshTokenDto refreshTokenDto);
    Task<ResponseDto> UploadAvatar(ClaimsPrincipal userPrincipal, IFormFile avatarFile);
}