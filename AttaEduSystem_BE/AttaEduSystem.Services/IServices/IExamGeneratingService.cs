using AttaEduSystem.Models.DTOs;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamGeneratingService
    {
        Task<ResponseDto> GenerateSimilarExam(Guid originalExamPaperId, ClaimsPrincipal user);
    }
}
