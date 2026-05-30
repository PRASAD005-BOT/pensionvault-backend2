using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class CorpusRecord
{
    public Guid CorpusId { get; set; } = Guid.NewGuid();
    public Guid SchemeId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal TotalContributions { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal InvestmentIncome { get; set; }
    public decimal ManagementExpenses { get; set; }
    public decimal ClosingCorpus { get; set; }
    public CorpusStatus Status { get; set; } = CorpusStatus.Draft;

    // Navigation
    public FundScheme Scheme { get; set; } = null!;
}
