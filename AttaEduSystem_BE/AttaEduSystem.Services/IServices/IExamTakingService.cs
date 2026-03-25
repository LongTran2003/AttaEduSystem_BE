using System.Security.Claims;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.SubmitExam;

namespace AttaEduSystem.Services.IServices;

public interface IExamTakingService
{
    Task<ResponseDto> SubmitExam(SubmitExamDto submitDto, ClaimsPrincipal user);
    Task<ResponseDto> GetExamHistory(ClaimsPrincipal user);
    Task<ResponseDto> GetExamResult(Guid attemptId, ClaimsPrincipal user);
    Task<ResponseDto> AutoSaveAnswer(Guid examAttemptId, AutoSaveAnswerDto dto, ClaimsPrincipal user);
    Task<ResponseDto> GetAttemptLiveStatus(Guid examAttemptId, ClaimsPrincipal user);

    /// <summary>
    /// Auto-submit exam when time's up (submit answered questions only)
    /// </summary>
    Task<ResponseDto> AutoSubmitExam(Guid examAttemptId, ClaimsPrincipal user);
}
