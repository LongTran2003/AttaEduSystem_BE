using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AttaEduSystem.API.Hubs.Exam
{
    [Authorize]
    public class ExamHub : Hub
    {
        // =========================================================
        // JOIN EXAM ROOM GROUP
        // =========================================================
        public async Task JoinExamRoom(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomCode}");
            await Clients.Caller.SendAsync("JoinedRoom", new
            {
                RoomCode = roomCode,
                Message = $"Successfully joined room {roomCode}",
                Context.ConnectionId
            });
        }

        // =========================================================
        // LEAVE EXAM ROOM GROUP
        // =========================================================
        public async Task LeaveExamRoom(string roomCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomCode}");
            await Clients.Caller.SendAsync("LeftRoom", new
            {
                RoomCode = roomCode,
                Message = $"Left room {roomCode}"
            });
        }

        // =========================================================
        // CONNECTION EVENTS
        // =========================================================
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", new
            {
                Context.ConnectionId,
                UserId = Context.UserIdentifier,
                Message = "Connected to ExamHub"
            });
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Có thể log hoặc xử lý khi user disconnect
            await base.OnDisconnectedAsync(exception);
        }
    }
}
