using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IUserUsageRepository : IRepository<UserUsage>
    {
        Task<UserUsage?> GetCurrentPeriodAsync(string userId, DateTime now);
        void Update (UserUsage userUsage);
    }
}
