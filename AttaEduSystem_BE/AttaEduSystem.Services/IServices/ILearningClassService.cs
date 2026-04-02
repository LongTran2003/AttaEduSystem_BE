using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.LearningClass;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices;

public interface ILearningClassService
{
    Task<ResponseDto> CreateClass(CreateLearningClassDto dto, ClaimsPrincipal user);
    Task<ResponseDto> GetMyClasses(ClaimsPrincipal user, string? keyword = null, string? status = null);
    Task<ResponseDto> GetClassDetail(Guid classId, ClaimsPrincipal user);
    Task<ResponseDto> UpdateClass(Guid classId, UpdateLearningClassDto dto, ClaimsPrincipal user);
    Task<ResponseDto> DeleteClass(Guid classId, ClaimsPrincipal user);
    Task<ResponseDto> AddMembers(Guid classId, AddLearningClassMembersDto dto, ClaimsPrincipal user);
    Task<ResponseDto> JoinClassByEnrollKey(JoinClassByEnrollKeyDto dto, ClaimsPrincipal user);
    Task<ResponseDto> RegenerateEnrollKey(Guid classId, ClaimsPrincipal user);
    Task<ResponseDto> RemoveMember(Guid classId, string memberUserId, ClaimsPrincipal user);
    Task<ResponseDto> GetAdminClassOverview();
    Task<ResponseDto> GetAdminClassDashboard(int? month, int? year, Guid? classId, string? subject);
}
