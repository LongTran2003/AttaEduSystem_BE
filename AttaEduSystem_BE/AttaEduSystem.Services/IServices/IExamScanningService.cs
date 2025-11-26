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
        Task<ResponseDto> GetExamPapersByUser(ClaimsPrincipal user);

    }
}
