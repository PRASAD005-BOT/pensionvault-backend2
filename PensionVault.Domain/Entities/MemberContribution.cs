using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class MemberContribution
{
    public Guid ContributionId { get; set; } = Guid.NewGuid();
    public Guid RemittanceId { get; set; }
    public Guid MemberId { get; set; }
    public string Period { get; set; } = string.Empty; // YYYY-MM
    public decimal EmployeeAmount { get; set; }
    public decimal EmployerAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    public ContributionStatus Status { get; set; } = ContributionStatus.Pending;

    // Navigation
    public ContributionRemittance Remittance { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
