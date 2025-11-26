using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamScanningService
    {
        Task<ResponseDto> ScanExamPaper(UploadExamPaperDto uploadDto, ClaimsPrincipal user);
        Task<ResponseDto> GetScannedText(Guid examPaperId);
        Task<ResponseDto> GetExamPaperById(Guid examPaperId);
        Task<ResponseDto> GetAllReadyExamPapers(
            int pageNumber = 1,
            int pageSize = 10,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null);
        Task<ResponseDto> GetExamPapersByUser(ClaimsPrincipal user);
        Task<ResponseDto> GetExamPaperImage(Guid examPaperId);
        Task<ResponseDto> UpdateExamPaperStatus(Guid examPaperId, string status, ClaimsPrincipal user);

    }
}
