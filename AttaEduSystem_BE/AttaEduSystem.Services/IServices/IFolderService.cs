using System.Security.Claims;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Folder;

namespace AttaEduSystem.Services.IServices;

public interface IFolderService
{
    Task<ResponseDto> CreateFolder(CreateFolderDto dto, ClaimsPrincipal user);
    Task<ResponseDto> UpdateFolder(Guid folderId, UpdateFolderDto dto, ClaimsPrincipal user);
    Task<ResponseDto> DeleteFolder(Guid folderId, ClaimsPrincipal user);
    Task<ResponseDto> GetMyFolders(ClaimsPrincipal user);
    Task<ResponseDto> GetFolderDetails(Guid folderId, ClaimsPrincipal user); // Lấy danh sách đề trong folder
    Task<ResponseDto> AddExamToFolder(Guid folderId, AddExamToFolderDto dto, ClaimsPrincipal user);
    Task<ResponseDto> RemoveExamFromFolder(Guid examPaperId, ClaimsPrincipal user);
}