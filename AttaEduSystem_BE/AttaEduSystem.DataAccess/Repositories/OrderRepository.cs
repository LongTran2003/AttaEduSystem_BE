using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDBContext _context;

        public OrderRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<long> GenerateUniqueNumberAsync()
        {
            var maxOrderNumber = await _context.Orders.MaxAsync(o => (long?)o.OrderNumber) ?? 0;
            return maxOrderNumber + 1;
        }

        public async Task<Order?> GetByOrderNumberAsync(long orderNumber)
        {
            return await _context.Orders
                .Include(o => o.Plan)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();
        }
    }
}
