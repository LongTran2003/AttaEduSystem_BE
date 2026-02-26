namespace AttaEduSystem.Services.IServices
{
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(string userId, object notificationData);
    }
}
