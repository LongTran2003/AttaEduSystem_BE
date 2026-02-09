using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Admin;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IAdminService
    {
        // User Management
        Task<ResponseDto> GetAllUsers(UserFilterDto filterDto);
        Task<ResponseDto> GetUserById(string userId);
        Task<ResponseDto> UpdateUser(string userId, UpdateUserDto updateDto, ClaimsPrincipal admin);
        Task<ResponseDto> DeleteUser(string userId, ClaimsPrincipal admin);
        Task<ResponseDto> LockUser(string userId, int lockDurationDays, ClaimsPrincipal admin);
        Task<ResponseDto> UnlockUser(string userId, ClaimsPrincipal admin);
    }
}
