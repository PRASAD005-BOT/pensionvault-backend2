using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class ClaimDisbursement
{
    public Guid DisbursementId { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public Guid MemberId { get; set; }
    public decimal DisbursedAmount { get; set; }
    public decimal TaxDeducted { get; set; }
    public decimal NetAmount { get; set; }
    public string? BankAccountRef { get; set; }
    public DateTime? DisbursedDate { get; set; }
    public DisbursementStatus Status { get; set; } = DisbursementStatus.Pending;

    // Navigation
    public BenefitClaim Claim { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
