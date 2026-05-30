using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationCategory Category { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
