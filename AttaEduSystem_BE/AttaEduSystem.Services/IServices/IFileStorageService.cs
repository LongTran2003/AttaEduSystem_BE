using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.FileStorage;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IFileStorageService
    {
        Task<ResponseDto> UploadAvatarImage(UploadFileDto uploadFileDto, ClaimsPrincipal user);
    }
}
