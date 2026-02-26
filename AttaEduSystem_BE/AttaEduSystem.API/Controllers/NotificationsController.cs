using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    [SwaggerTag("Notification Management APIs")]

    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // =========================================================
        // GET: /api/notifications
        // =========================================================
        /// <summary>
        /// Lấy danh sách notifications của user hiện tại (có phân trang)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var response = await _notificationService.GetNotificationsAsync(User, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // =========================================================
        // GET: /api/notifications/unread-count
        // =========================================================
        /// <summary>
        /// Lấy số lượng notifications chưa đọc của user hiện tại
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var response = await _notificationService.GetUnreadCountAsync(User);
            return StatusCode(response.StatusCode, response);
        }

        // =========================================================
        // PUT: /api/notifications/{id}/read
        // =========================================================
        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var response = await _notificationService.MarkAsReadAsync(id, User);
            return StatusCode(response.StatusCode, response);
        }
    }
}
