using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class Member
{
    public Guid MemberId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? NationalIdRef { get; set; }
    public Guid EmployerId { get; set; }
    public DateTime JoiningDate { get; set; }
    public DateTime? DateOfRetirement { get; set; }
    public string? NomineeDetails { get; set; } // JSON
    public MemberStatus Status { get; set; } = MemberStatus.Active;

    // Navigation
    public User User { get; set; } = null!;
    public Employer Employer { get; set; } = null!;
    public ICollection<FundAccount> FundAccounts { get; set; } = new List<FundAccount>();
    public ICollection<MemberContribution> Contributions { get; set; } = new List<MemberContribution>();
    public ICollection<BenefitClaim> Claims { get; set; } = new List<BenefitClaim>();
    public ICollection<AnnuityPlan> AnnuityPlans { get; set; } = new List<AnnuityPlan>();
}
