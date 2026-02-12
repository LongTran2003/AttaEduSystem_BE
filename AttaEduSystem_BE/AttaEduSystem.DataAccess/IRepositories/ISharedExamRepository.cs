using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface ISharedExamRepository : IRepository<SharedExam>
    {
        Task<SharedExam?> GetByTokenAsync(string token);
        Task<IEnumerable<SharedExam>> GetByUserIdAsync(string userId);
        Task<bool> TokenExistsAsync(string token);
        void Update(SharedExam sharedExam);
    }
}
