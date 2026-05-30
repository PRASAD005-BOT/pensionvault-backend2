using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class MonthlyPensionDisbursement
{
    public Guid DisbursementId { get; set; } = Guid.NewGuid();
    public Guid AnnuityId { get; set; }
    public Guid MemberId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TaxDeducted { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime? DisbursedDate { get; set; }
    public PensionDisbursementStatus Status { get; set; } = PensionDisbursementStatus.Pending;

    // Navigation
    public AnnuityPlan AnnuityPlan { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
