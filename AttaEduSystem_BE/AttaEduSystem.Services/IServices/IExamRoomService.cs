using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamRoom.Room;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamRoomService
    {
        // Room Management (Teacher/Admin)
        Task<ResponseDto> CreateRoom(CreateExamRoomDto dto, ClaimsPrincipal user);
        Task<ResponseDto> CreateRoomsBatch(List<CreateExamRoomDto> dtos, ClaimsPrincipal user);
        Task<ResponseDto> GetMyRooms(ClaimsPrincipal user);
        Task<ResponseDto> CancelRoom(Guid examRoomId, ClaimsPrincipal user);
        Task<ResponseDto> GetSelectedExamByRoomId(Guid examRoomId, ClaimsPrincipal user);
        Task<ResponseDto> GetRoomResults(Guid examRoomId, ClaimsPrincipal user);
        Task<ResponseDto> GetTeacherDashboard(DateTime? date, ClaimsPrincipal user);
        Task<ResponseDto> UpdateRoomSchedule(Guid examRoomId, DateTime newStartTime, int? timeLimit, ClaimsPrincipal user);

        // Public APIs
        Task<ResponseDto> GetRoomByCode(string code);
        Task<ResponseDto> GetRoomStatus(string code);

        // Student APIs
        Task<ResponseDto> JoinRoom(string code, ClaimsPrincipal user);
        Task<ResponseDto> GetPaperForTaking(string code, ClaimsPrincipal user);
        Task<ResponseDto> JoinRoomById(Guid examRoomId, ClaimsPrincipal user);
        Task<ResponseDto> GetPaperForTakingByRoomId(Guid examRoomId, ClaimsPrincipal user);
    }
}
