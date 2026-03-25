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
        Task<ResponseDto> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto, ClaimsPrincipal adminUser);
        Task<ResponseDto> UpdateSubscriptionPlan(Guid planId, UpdateSubscriptionPlanDto dto, ClaimsPrincipal adminUser);
        Task<ResponseDto> UpdateStatusSubscriptionPlan(Guid planId, UpdateStatusSubscriptionPlanDto dto, ClaimsPrincipal adminUser);
        Task<ResponseDto> DeleteSubscriptionPlan(Guid planId, ClaimsPrincipal adminUser);
    }
}
