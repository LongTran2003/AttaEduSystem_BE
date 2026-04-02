using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubService _notificationHubService;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationHubService notificationHubService,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationHubService = notificationHubService;
            _emailService = emailService;
            _logger = logger;
        }

        // =========================================================
        // CREATE AND SEND NOTIFICATION
        // =========================================================
        public async Task CreateAndSendNotificationAsync(
            string userId,
            string title,
            string message,
            string type,
            string? actionUrl = null,
            string? emailAddress = null)
        {
            try
            {
                // 1. Tạo notification entity
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    NotificationType = type,
                    ActionUrl = actionUrl,
                    IsRead = false,
                    CreatedAt = StaticOperationStatus.Timezone.Vietnam,
                    CreatedBy = "AttaEdu-System",
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                    Status = "Active"
                };

                // 2. Lưu vào DB
                await _unitOfWork.Notification.AddAsync(notification);
                await _unitOfWork.SaveAsync();

                // 3. Gửi qua SignalR (Real-time Web) thông qua Service trung gian
                try
                {
                    // [SỬA ĐỔI]: Sử dụng hàm từ INotificationHubService
                    await _notificationHubService.SendNotificationToUserAsync(userId, new
                    {
                        notification.NotificationId,
                        notification.Title,
                        notification.Message,
                        notification.NotificationType,
                        notification.ActionUrl,
                        notification.CreatedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send SignalR notification to user {UserId}", userId);
                }

                // 4. Gửi email (nếu có) - Fire and forget
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var emailBody = $@"
                                <h2>{title}</h2>
                                <p>{message}</p>
                                {(string.IsNullOrEmpty(actionUrl) ? "" : $"<p><a href='{actionUrl}'>Xem chi tiết</a></p>")}
                            ";

                            await _emailService.SendEmailAsync(emailAddress, title, emailBody);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send email notification to {Email}", emailAddress);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification for user {UserId}", userId);
                throw;
            }
        }

        // =========================================================
        // GET NOTIFICATIONS (PAGINATED)
        // =========================================================
        public async Task<ResponseDto> GetNotificationsAsync(ClaimsPrincipal user, int page = 1, int pageSize = 20)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
            }

            var (notifications, totalCount) = await _unitOfWork.Notification
                .GetPaginatedByUserIdAsync(userId, page, pageSize);

            return SuccessResponse.Build(
                message: "Notifications retrieved successfully",
                statusCode: 200,
                result: new
                {
                    Data = notifications.Select(n => new
                    {
                        n.NotificationId,
                        n.Title,
                        n.Message,
                        n.NotificationType,
                        n.ActionUrl,
                        n.IsRead,
                        n.CreatedAt
                    }),
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasPreviousPage = page > 1,
                    HasNextPage = page * pageSize < totalCount,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        HasPreviousPage = page > 1,
                        HasNextPage = page * pageSize < totalCount
                    }
                });
        }

        // =========================================================
        // GET UNREAD COUNT
        // =========================================================
        public async Task<ResponseDto> GetUnreadCountAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
            }

            var unreadCount = await _unitOfWork.Notification.GetUnreadCountByUserIdAsync(userId);

            return SuccessResponse.Build(
                message: "Unread count retrieved successfully",
                statusCode: 200,
                result: new { UnreadCount = unreadCount });
        }

        // =========================================================
        // MARK AS READ
        // =========================================================
        public async Task<ResponseDto> MarkAsReadAsync(Guid notificationId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
            }

            var notification = await _unitOfWork.Notification
                .GetAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return ErrorResponse.Build("Notification not found", 404);
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.UpdatedBy = user.FindFirstValue("FullName");
                notification.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                _unitOfWork.Notification.Update(notification);
                await _unitOfWork.SaveAsync();
            }

            return SuccessResponse.Build("Notification marked as read", 200);
        }
    }
}
