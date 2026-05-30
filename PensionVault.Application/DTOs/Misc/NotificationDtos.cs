using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Notifications;

public record NotificationResponse(
    Guid NotificationId,
    Guid UserId,
    string Message,
    NotificationCategory Category,
    NotificationStatus Status,
    DateTime CreatedDate
);
