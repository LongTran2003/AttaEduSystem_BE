using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamQuestionService
    {
        // CRUD Operations
        Task<ResponseDto> GetQuestionById(Guid questionId);
        Task<ResponseDto> UpdateQuestion(Guid questionId, UpdateExamQuestionDto dto, ClaimsPrincipal user);
        Task<ResponseDto> DeleteQuestion(Guid questionId, ClaimsPrincipal user);

        // ExamPaper - Question Operations
        Task<ResponseDto> AddQuestionToExamPaper(Guid examPaperId, AddExamQuestionDto dto, ClaimsPrincipal user);
        Task<ResponseDto> ReorderQuestions(Guid examPaperId, ReorderQuestionsDto dto, ClaimsPrincipal user);
        Task<ResponseDto> GetQuestionsByExamPaper(Guid examPaperId);
    }
}
