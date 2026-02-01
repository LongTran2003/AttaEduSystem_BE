using System.Security.Claims;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Profile;

namespace AttaEduSystem.Services.IServices;

public interface IProfileService
{
    Task<ResponseDto> GetUserProfile(ClaimsPrincipal userPrincipal);
    Task<ResponseDto> UpdateUserProfile(ClaimsPrincipal userPrincipal, UpdateUserProfileDto updateUserProfileDto);
}