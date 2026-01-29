using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface ISubscriptionService
    {
        Task<ResponseDto> GetAvailablePlans();
        Task<ResponseDto> GetCurrentSubscription(ClaimsPrincipal user);
        Task<ResponseDto> ActivateFromOrder(Guid orderId);
        Task<bool> CanUseAdvancedFeature(string userId, string featureName);
        Task<ResponseDto> CreateCheckout(ClaimsPrincipal user, CreateCheckoutRequestDto request);
    }
}
