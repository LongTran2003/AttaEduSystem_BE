using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
    {
        Task<SubscriptionPlan?> GetByCodeAsync(string code);
        Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync();
        void Update(SubscriptionPlan subscriptionPlan);
    }
}
