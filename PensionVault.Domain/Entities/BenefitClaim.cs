using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class BenefitClaim
{
    public Guid ClaimId { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public ClaimType ClaimType { get; set; }
    public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
    public decimal EligibleAmount { get; set; }
    public decimal VestedAmount { get; set; }
    public decimal TaxDeductible { get; set; }
    public Guid? ProcessedById { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

    // Navigation
    public Member Member { get; set; } = null!;
    public User? ProcessedBy { get; set; }
    public ICollection<ClaimDisbursement> Disbursements { get; set; } = new List<ClaimDisbursement>();
}
