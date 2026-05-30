using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class InvestmentPortfolio
{
    public Guid PortfolioId { get; set; } = Guid.NewGuid();
    public Guid SchemeId { get; set; }
    public AssetClass AssetClass { get; set; }
    public decimal AllocationPercent { get; set; }
    public decimal InvestedValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal YieldEarned { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation
    public FundScheme Scheme { get; set; } = null!;
}
