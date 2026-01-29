using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Models.DTOs.Payment;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IPayOsService
    {
        Task<ResponseDto> CreatePayOsPaymentLink(ClaimsPrincipal user, CreatePaymentLinkDto createPaymentLinkDto);
        Task<ResponseDto> ConfirmPayOsTransaction(ConfirmPaymentDto confirmPaymentDto);
        Task<ResponseDto> GetAllPayments(
        ClaimsPrincipal user,
        int pageNumber = 1,
        int pageSize = 10,
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null);
        Task<ResponseDto> GetPaymentById(ClaimsPrincipal user, Guid paymentTransactionId);
    }
}
