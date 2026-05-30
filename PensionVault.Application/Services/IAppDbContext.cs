using Microsoft.EntityFrameworkCore;
using PensionVault.Domain.Entities;

namespace PensionVault.Application.Services;

/// <summary>
/// Interface for the EF Core DbContext used across Application services.
/// Allows Application layer to depend on an abstraction, not the concrete Infrastructure type.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<FundScheme> FundSchemes { get; }
    DbSet<Employer> Employers { get; }
    DbSet<Member> Members { get; }
    DbSet<FundAccount> FundAccounts { get; }
    DbSet<ContributionRemittance> ContributionRemittances { get; }
    DbSet<MemberContribution> MemberContributions { get; }
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<InterestCreditRecord> InterestCreditRecords { get; }
    DbSet<BenefitClaim> BenefitClaims { get; }
    DbSet<ClaimDisbursement> ClaimDisbursements { get; }
    DbSet<InvestmentPortfolio> InvestmentPortfolios { get; }
    DbSet<CorpusRecord> CorpusRecords { get; }
    DbSet<AnnuityPlan> AnnuityPlans { get; }
    DbSet<MonthlyPensionDisbursement> MonthlyPensionDisbursements { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
