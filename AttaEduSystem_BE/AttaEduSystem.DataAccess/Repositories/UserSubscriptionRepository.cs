using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class UserSubscriptionRepository : Repository<UserSubscription>, IUserSubscriptionRepository
    {
        private readonly ApplicationDBContext _context;

        public UserSubscriptionRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserSubscription?> GetActiveByUserIdAsync(string userId)
        {
            return await _context.UserSubscriptions
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == "Active");
        }

        public void Update(UserSubscription userSubscription)
        {
            _context.UserSubscriptions.Attach(userSubscription);
            _context.Entry(userSubscription).State = EntityState.Modified;
        }
    }
}
