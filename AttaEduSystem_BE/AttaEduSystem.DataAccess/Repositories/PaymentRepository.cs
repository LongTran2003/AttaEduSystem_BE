using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDBContext _context;

        public PaymentRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Payment?> GetLatestPendingByOrderNumberAsync(long orderNumber)
        {
            return await _context.Payments
                .Where(p => p.OrderNumber == orderNumber && p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Payment>> GetByOrderNumberAsync(long orderNumber)
        {
            return await _context.Payments
                .Where(p => p.OrderNumber == orderNumber)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
