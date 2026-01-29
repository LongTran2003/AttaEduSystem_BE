using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetPaymentByOrderNumberAsync(long orderNumber);
        Task<(List<Payment> Payments, int TotalPayments)> GetPaymentsAsync(
            int pageNumber,
            int pageSize,
            string? filterOn,
            string? filterQuery,
            string? sortBy,
            string? userId = null);
        void Update(Payment payment);

    }
}
