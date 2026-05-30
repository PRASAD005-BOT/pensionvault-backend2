using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class InterestCreditRecord
{
    public Guid InterestId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string FinancialYear { get; set; } = string.Empty; // e.g. 2024-25
    public decimal OpeningBalance { get; set; }
    public decimal TotalContributions { get; set; }
    public decimal InterestRateApplied { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public DateTime CreditedDate { get; set; }
    public InterestCreditStatus Status { get; set; } = InterestCreditStatus.Computed;

    // Navigation
    public FundAccount Account { get; set; } = null!;
}
