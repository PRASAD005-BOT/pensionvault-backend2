using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Ledger;

public record LedgerEntryResponse(
    Guid EntryId,
    Guid AccountId,
    EntryType EntryType,
    decimal Amount,
    decimal BalanceAfter,
    DateTime EntryDate,
    string? ReferenceId,
    LedgerEntryStatus Status
);

public record CreditInterestRequest(
    Guid AccountId,
    string FinancialYear,
    decimal InterestRate
);

public record InterestCreditResponse(
    Guid InterestId,
    Guid AccountId,
    string FinancialYear,
    decimal OpeningBalance,
    decimal TotalContributions,
    decimal InterestRateApplied,
    decimal InterestAmount,
    decimal ClosingBalance,
    DateTime CreditedDate,
    InterestCreditStatus Status
);

public record FundAccountResponse(
    Guid AccountId,
    Guid MemberId,
    string MemberName,
    Guid SchemeId,
    string SchemeName,
    DateTime AccountOpenDate,
    decimal EmployeeContributionBalance,
    decimal EmployerContributionBalance,
    decimal InterestAccrued,
    decimal TotalBalance,
    decimal VestingPercent,
    FundAccountStatus Status
);
