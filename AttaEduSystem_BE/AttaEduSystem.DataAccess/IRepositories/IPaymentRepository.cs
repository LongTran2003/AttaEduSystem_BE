using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetLatestPendingByOrderNumberAsync(long orderNumber);
        Task<IEnumerable<Payment>> GetByOrderNumberAsync(long orderNumber);
    }
}
