namespace AttaEduSystem.Services.IServices
{
    public interface IExamHubService
    {
        /// <summary>
        /// Broadcast timer update to all participants in a room
        /// </summary>
        Task SendTimerUpdate(string roomCode, int remainingSeconds);

        /// <summary>
        /// Broadcast room status change (Started, Finished, etc.)
        /// </summary>
        Task SendRoomStatusChanged(string roomCode, string status, string message);

        /// <summary>
        /// Force all participants to submit their exams
        /// </summary>
        Task ForceSubmitAll(string roomCode);

        /// <summary>
        /// Notify a specific user
        /// </summary>
        Task NotifyUser(string userId, string eventName, object data);
    }
}
