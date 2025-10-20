using System.Linq.Expressions;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        void Remove(T entity);
        Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null);
        void RemoveRange(IEnumerable<T> entities);
    }
}
