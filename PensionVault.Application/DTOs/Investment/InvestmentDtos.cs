using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Investment;

public record CreatePortfolioRequest(
    Guid SchemeId,
    AssetClass AssetClass,
    decimal AllocationPercent,
    decimal InvestedValue,
    decimal CurrentValue,
    decimal YieldEarned
);

public record UpdatePortfolioRequest(
    decimal AllocationPercent,
    decimal InvestedValue,
    decimal CurrentValue,
    decimal YieldEarned
);

public record PortfolioResponse(
    Guid PortfolioId,
    Guid SchemeId,
    string SchemeName,
    AssetClass AssetClass,
    decimal AllocationPercent,
    decimal InvestedValue,
    decimal CurrentValue,
    decimal YieldEarned,
    DateTime LastUpdated
);

public record CreateCorpusRequest(
    Guid SchemeId,
    DateTime RecordDate,
    decimal TotalContributions,
    decimal TotalWithdrawals,
    decimal InvestmentIncome,
    decimal ManagementExpenses
);

public record CorpusResponse(
    Guid CorpusId,
    Guid SchemeId,
    string SchemeName,
    DateTime RecordDate,
    decimal TotalContributions,
    decimal TotalWithdrawals,
    decimal InvestmentIncome,
    decimal ManagementExpenses,
    decimal ClosingCorpus,
    CorpusStatus Status
);
