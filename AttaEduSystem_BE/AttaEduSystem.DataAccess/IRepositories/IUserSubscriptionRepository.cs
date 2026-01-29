using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IUserSubscriptionRepository : IRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActiveByUserIdAsync(string userId);
    }
}
