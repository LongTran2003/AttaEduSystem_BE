using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        /// <summary>
        /// Lấy danh sách notification của user với phân trang, sắp xếp theo CreatedAt giảm dần
        /// </summary>
        Task<(List<Notification> Notifications, int TotalCount)> GetPaginatedByUserIdAsync(
            string userId,
            int page,
            int pageSize);

        /// <summary>
        /// Đếm số lượng notification chưa đọc của user
        /// </summary>
        Task<int> GetUnreadCountByUserIdAsync(string userId);

        void Update(Notification notification);
    }
}
