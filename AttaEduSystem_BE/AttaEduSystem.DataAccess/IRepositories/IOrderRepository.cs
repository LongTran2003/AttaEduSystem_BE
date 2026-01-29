using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByOrderNumberAsync(long orderNumber);
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
        Task<long> GenerateUniqueNumberAsync();
    }
}
