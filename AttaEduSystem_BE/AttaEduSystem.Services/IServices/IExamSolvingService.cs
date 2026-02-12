using AttaEduSystem.Models.DTOs;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamSolvingService
    {
        Task<ResponseDto> SolveExamPaper(Guid examPaperId, ClaimsPrincipal user);
        Task<ResponseDto> GetSolutionByExamId(Guid examPaperId);
        Task<ResponseDto> UpdateStatus(Guid solutionId, string status, ClaimsPrincipal user);
        Task<ResponseDto> GetAllSolutions(
            int pageNumber = 1,
            int pageSize = 10,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null);

        Task<ResponseDto> GetSolutionById(Guid solutionId);
        Task<ResponseDto> GetSolutionsByUser(ClaimsPrincipal user);
    }
}
