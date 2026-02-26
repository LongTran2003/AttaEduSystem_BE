using AttaEduSystem.Models.DTOs;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface INotificationService
    {
        /// <summary>
        /// Tạo notification mới và gửi qua SignalR + Email (nếu có)
        /// </summary>
        Task CreateAndSendNotificationAsync(
            string userId,
            string title,
            string message,
            string type,
            string? actionUrl = null,
            string? emailAddress = null);

        /// <summary>
        /// Lấy danh sách notifications của user (có phân trang)
        /// </summary>
        Task<ResponseDto> GetNotificationsAsync(ClaimsPrincipal user, int page = 1, int pageSize = 20);

        /// <summary>
        /// Đếm số lượng notification chưa đọc
        /// </summary>
        Task<ResponseDto> GetUnreadCountAsync(ClaimsPrincipal user);

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        Task<ResponseDto> MarkAsReadAsync(Guid notificationId, ClaimsPrincipal user);
    }
}
