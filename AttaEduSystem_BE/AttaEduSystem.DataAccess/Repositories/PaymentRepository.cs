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

        public async Task<Payment?> GetPaymentByOrderNumberAsync(long orderNumber)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderNumber == orderNumber);
        }

        public async Task<(List<Payment> Payments, int TotalPayments)> GetPaymentsAsync(
            int pageNumber,
            int pageSize,
            string? filterOn,
            string? filterQuery,
            string? sortBy,
            string? userId = null)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .AsQueryable();

            // Lọc theo user nếu cần (user chỉ thấy payment của mình)
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.Order.UserId == userId);
            }

            // Filter
            if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
            {
                switch (filterOn.ToLower())
                {
                    case "status":
                        if (Enum.TryParse<PaymentStatus>(filterQuery, true, out var status))
                            query = query.Where(p => p.Status == status);
                        break;

                    case "amount":
                        if (decimal.TryParse(filterQuery, out var amount))
                            query = query.Where(p => p.Amount == amount);
                        break;

                    case "ordernumber":
                        if (long.TryParse(filterQuery, out var orderNo))
                            query = query.Where(p => p.OrderNumber == orderNo);
                        break;
                }
            }

            var totalPayments = await query.CountAsync();

            // Sort
            query = sortBy?.ToLower() switch
            {
                "amount" => query.OrderBy(p => p.Amount),
                "amount_desc" => query.OrderByDescending(p => p.Amount),
                "status" => query.OrderBy(p => p.Status),
                "createdat" => query.OrderBy(p => p.CreatedAt),
                "createdat_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var payments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (payments, totalPayments);
        }

        public void Update(Payment payment)
        {
            _context.Payments.Update(payment);
        }
    }
}
