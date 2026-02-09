using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamShuffle;
using AttaEduSystem.Models.DTOs.QuestionBank;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamShuffleService
    {
        Task<ResponseDto> ShuffleExam(Guid examPaperId, ShuffleExamRequestDto dto, ClaimsPrincipal user);
        Task<ResponseDto> GetShuffledVariants(Guid examPaperId, ClaimsPrincipal user);
        Task<ResponseDto> SearchQuestionBank(QuestionBankFilterDto filterDto, ClaimsPrincipal user);
    }
}
