using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.Models.Entities
{
    public class Notification : BaseEntity<string, string, string>
    {
        public Guid NotificationId { get; set; }
        public string UserId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = StaticOperationStatus.Timezone.Vietnam;
        public string NotificationType { get; set; } = "System";

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
