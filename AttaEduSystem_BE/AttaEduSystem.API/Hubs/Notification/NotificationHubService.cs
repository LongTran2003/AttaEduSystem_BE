using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.SignalR;

namespace AttaEduSystem.API.Hubs.Notification
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(string userId, object notificationData)
        {
            // Bắn tín hiệu SignalR tại đây, nơi nó hoàn toàn hiểu NotificationHub là gì
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notificationData);
        }
    }
}
