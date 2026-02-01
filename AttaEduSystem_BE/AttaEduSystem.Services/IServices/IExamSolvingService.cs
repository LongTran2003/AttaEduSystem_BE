using AttaEduSystem.Models.DTOs;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamSolvingService
    {
        Task<ResponseDto> SolveExamPaper(Guid examPaperId, ClaimsPrincipal user);
        Task<ResponseDto> GetSolutionByExamId(Guid examPaperId);
        Task<ResponseDto> UpdateStatus(Guid solutionId, string status, ClaimsPrincipal user);
    }
}
