using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class ContributionRemittance
{
    public Guid RemittanceId { get; set; } = Guid.NewGuid();
    public Guid EmployerId { get; set; }
    public string RemittancePeriod { get; set; } = string.Empty; // YYYY-MM
    public decimal TotalEmployeeShare { get; set; }
    public decimal TotalEmployerShare { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime RemittanceDate { get; set; }
    public int CoverageCount { get; set; }
    public RemittanceStatus Status { get; set; } = RemittanceStatus.Received;

    // Navigation
    public Employer Employer { get; set; } = null!;
    public ICollection<MemberContribution> MemberContributions { get; set; } = new List<MemberContribution>();
}
