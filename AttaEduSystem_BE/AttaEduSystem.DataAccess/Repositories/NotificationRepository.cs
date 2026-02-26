using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ApplicationDBContext _context;

        public NotificationRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<Notification> Notifications, int TotalCount)> GetPaginatedByUserIdAsync(
            string userId,
            int page,
            int pageSize)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task<int> GetUnreadCountByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public void Update (Notification notification)
        {
            _context.Notifications.Update(notification);
        }
    }
}
