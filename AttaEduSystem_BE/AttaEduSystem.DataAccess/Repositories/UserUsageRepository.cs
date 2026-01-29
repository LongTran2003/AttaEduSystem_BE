using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class UserUsageRepository : Repository<UserUsage>, IUserUsageRepository
    {
        private readonly ApplicationDBContext _context;

        public UserUsageRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserUsage?> GetCurrentPeriodAsync(string userId, DateTime now)
        {
            return await _context.UserUsages.FirstOrDefaultAsync(x =>
                x.UserId == userId && x.PeriodStart <= now && now <= x.PeriodEnd);
        }

        public void Update(UserUsage userUsage)
        {
            _context.UserUsages.Attach(userUsage);
            _context.Entry(userUsage).State = EntityState.Modified;
        }
    }
}
