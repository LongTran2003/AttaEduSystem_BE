using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.Enums;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IUsageTrackerService
    {
        Task<bool> TryConsumeAsync(ClaimsPrincipal user, UsageType type, int amount = 1);
        Task<ResponseDto> GetUsageInfo(ClaimsPrincipal user);
    }
}
