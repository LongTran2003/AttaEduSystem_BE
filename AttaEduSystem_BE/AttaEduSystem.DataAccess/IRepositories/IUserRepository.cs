using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetUserWithDetailsAsync(string userId);
        Task<(List<ApplicationUser> Users, int TotalCount)> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            string? status = null);
    }
}
