using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IOpenAiService
    {
        Task<ResponseDto> GenerateExamAsync(GenerateExamRequestDto requestDto, ClaimsPrincipal user);
        Task<ResponseDto> GetGeneratedExamAsync(Guid generatedExamId, ClaimsPrincipal user);
        Task<ResponseDto> GetGeneratedExamsByOriginalAsync(Guid examPaperId, ClaimsPrincipal user);
    }
}
