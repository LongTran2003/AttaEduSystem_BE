using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AttaEduSystem.API.Hubs.Notification
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // =========================================================
        // CONNECTION EVENTS
        // =========================================================
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            await Clients.Caller.SendAsync("Connected", new
            {
                Context.ConnectionId,
                UserId = userId,
                Message = "Connected to NotificationHub"
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (exception != null)
            {
                // Log error nếu cần
                Console.WriteLine($"User {userId} disconnected with error: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
