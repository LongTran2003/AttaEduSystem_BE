using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        private readonly ApplicationDBContext _context;

        public SubscriptionPlanRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SubscriptionPlan?> GetByCodeAsync(string code)
        {
            return await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(p => p.Status != null && p.Status == "Active")
                .OrderBy(p => p.PricePerMonth)
                .ToListAsync();
        }

        public void Update (SubscriptionPlan subscriptionPlan)
        {
            _context.SubscriptionPlans.Update(subscriptionPlan);
        }
    }
}
