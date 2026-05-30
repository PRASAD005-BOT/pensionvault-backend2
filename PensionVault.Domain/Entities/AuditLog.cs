using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class AuditLog
{
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
