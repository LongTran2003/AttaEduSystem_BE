using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamRoomRepository : IRepository<ExamRoom>
    {
        Task<ExamRoom?> GetByCodeAsync(string code);
        Task<ExamRoom?> GetByCodeWithParticipantsAsync(string code);
        Task<ExamRoom?> GetByIdWithParticipantsAsync(Guid examRoomId);
        Task<List<ExamRoom>> GetByCreatorIdAsync(string userId);
        Task<bool> IsCodeExistsAsync(string code);
        void Update(ExamRoom examRoom);
    }
}
