using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.SignalR;

namespace AttaEduSystem.API.Hubs.Exam
{
    public class ExamHubService : IExamHubService
    {
        private readonly IHubContext<ExamHub> _hubContext;

        public ExamHubService(IHubContext<ExamHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // =========================================================
        // SEND TIMER UPDATE TO ROOM
        // =========================================================
        public async Task SendTimerUpdate(string roomCode, int remainingSeconds)
        {
            await _hubContext.Clients.Group($"room_{roomCode}").SendAsync("TimerUpdate", new
            {
                RoomCode = roomCode,
                RemainingSeconds = remainingSeconds,
                FormattedTime = FormatTime(remainingSeconds),
                Timestamp = DateTime.UtcNow
            });
        }

        // =========================================================
        // SEND ROOM STATUS CHANGED
        // =========================================================
        public async Task SendRoomStatusChanged(string roomCode, string status, string message)
        {
            await _hubContext.Clients.Group($"room_{roomCode}").SendAsync("RoomStatusChanged", new
            {
                RoomCode = roomCode,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        // =========================================================
        // FORCE SUBMIT ALL PARTICIPANTS
        // =========================================================
        public async Task ForceSubmitAll(string roomCode)
        {
            await _hubContext.Clients.Group($"room_{roomCode}").SendAsync("ForceSubmit", new
            {
                RoomCode = roomCode,
                Message = "Time's up! Your exam will be auto-submitted.",
                Timestamp = DateTime.UtcNow
            });
        }

        // =========================================================
        // NOTIFY SPECIFIC USER
        // =========================================================
        public async Task NotifyUser(string userId, string eventName, object data)
        {
            await _hubContext.Clients.User(userId).SendAsync(eventName, data);
        }

        // =========================================================
        // HELPER: Format seconds to MM:SS
        // =========================================================
        private static string FormatTime(int totalSeconds)
        {
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
