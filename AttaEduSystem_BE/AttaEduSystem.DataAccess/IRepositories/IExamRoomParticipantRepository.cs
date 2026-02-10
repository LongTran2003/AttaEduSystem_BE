using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamRoomParticipantRepository : IRepository<ExamRoomParticipant>
    {
        Task<ExamRoomParticipant?> GetByRoomAndUserAsync(Guid examRoomId, string userId);
        Task<List<ExamRoomParticipant>> GetByRoomIdAsync(Guid examRoomId);
        Task<int> GetParticipantCountAsync(Guid examRoomId);
        void Update(ExamRoomParticipant participant);
    }
}
