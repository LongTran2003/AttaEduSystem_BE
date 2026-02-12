using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.SharedExam;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IShareService
    {
        Task<ResponseDto> CreateShareLink(CreateShareLinkDto dto, ClaimsPrincipal user);
        Task<ResponseDto> GetSharedExam(string token, string? password = null);
        Task<ResponseDto> GetMyShareLinks(ClaimsPrincipal user);
        Task<ResponseDto> DeactivateShareLink(Guid sharedExamId, ClaimsPrincipal user);
    }
}
