using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class Employer
{
    public Guid EmployerId { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public int EnrolledMemberCount { get; set; }
    public RemittanceFrequency RemittanceFrequency { get; set; } = RemittanceFrequency.Monthly;
    public string? ContactDetails { get; set; } // JSON
    public EmployerStatus Status { get; set; } = EmployerStatus.Active;

    // Navigation
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<ContributionRemittance> Remittances { get; set; } = new List<ContributionRemittance>();
}
