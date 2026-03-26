using System.Security.Claims;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Models.DTOs.ExamRoom.Room;
using AttaEduSystem.Models.DTOs.Folder;
using AttaEduSystem.Models.DTOs.ExamPaper;

namespace AttaEduSystem.Services.IServices;

public interface IFolderService
{
    Task<ResponseDto> CreateFolder(CreateFolderDto dto, ClaimsPrincipal user);
    Task<ResponseDto> UpdateFolder(Guid folderId, UpdateFolderDto dto, ClaimsPrincipal user);
    Task<ResponseDto> DeleteFolder(Guid folderId, ClaimsPrincipal user);
    Task<ResponseDto> GetMyFolders(ClaimsPrincipal user);
    Task<ResponseDto> GetFolderDetails(Guid folderId, ClaimsPrincipal user, bool includeQuestions = false); // Lấy danh sách đề trong folder
    Task<ResponseDto> AddExamToFolder(Guid folderId, AddExamToFolderDto dto, ClaimsPrincipal user);
    Task<ResponseDto> RemoveExamFromFolder(Guid examPaperId, ClaimsPrincipal user);
    Task<ResponseDto> GetFolderExamDetails(Guid folderId, Guid examPaperId, ClaimsPrincipal user);
    Task<ResponseDto> AddQuestionToFolderExam(Guid folderId, Guid examPaperId, AddExamQuestionDto dto, ClaimsPrincipal user);
    Task<ResponseDto> UpdateQuestionInFolderExam(Guid folderId, Guid examPaperId, Guid questionId, UpdateExamQuestionDto dto, ClaimsPrincipal user);
    Task<ResponseDto> DeleteQuestionInFolderExam(Guid folderId, Guid examPaperId, Guid questionId, ClaimsPrincipal user);
    Task<ResponseDto> UpdateQuestionsInFolderExam(Guid folderId, Guid examPaperId, BatchUpdateQuestionsDto dto, ClaimsPrincipal user);
    Task<ResponseDto> SolveExamInFolder(Guid folderId, Guid examPaperId, ClaimsPrincipal user);
    Task<ResponseDto> GetExamSolutionInFolder(Guid folderId, Guid examPaperId, ClaimsPrincipal user);
    Task<ResponseDto> CreateQuizRoomFromFolderExam(Guid folderId, Guid examPaperId, CreateExamRoomDto dto, ClaimsPrincipal user);
}
