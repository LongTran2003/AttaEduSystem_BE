using AttaEduSystem.Models.Entities;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateJwtAccessTokenAsync(ApplicationUser user);
        Task<string> GenerateJwtRefreshTokenAsync(ApplicationUser user);
        Task<bool> StoreRefreshToken(string userId, string refreshToken);
        Task<ClaimsPrincipal> GetPrincipalFromToken(string token);
        Task<string> RetrieveRefreshTokenAsync(string userId);
    }
}
