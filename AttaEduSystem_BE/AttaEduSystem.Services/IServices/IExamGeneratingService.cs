using AttaEduSystem.Models.DTOs;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamGeneratingService
    {
        Task<ResponseDto> GenerateSimilarExam(Guid originalExamPaperId, ClaimsPrincipal user);
        Task<ResponseDto> UpdateStatus(Guid generatedExamId, string status, ClaimsPrincipal user);
        Task<ResponseDto> GetAllGeneratedExams(
            int pageNumber = 1,
            int pageSize = 10,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null);

        Task<ResponseDto> GetGeneratedExamById(Guid generatedExamId);
        Task<ResponseDto> GetGeneratedExamsByUser(ClaimsPrincipal user);
    }
}
