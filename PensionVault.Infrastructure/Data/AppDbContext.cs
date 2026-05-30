using Microsoft.EntityFrameworkCore;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Infrastructure.Data;

public class AppDbContext : DbContext, PensionVault.Application.Services.IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FundScheme> FundSchemes => Set<FundScheme>();
    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<FundAccount> FundAccounts => Set<FundAccount>();
    public DbSet<ContributionRemittance> ContributionRemittances => Set<ContributionRemittance>();
    public DbSet<MemberContribution> MemberContributions => Set<MemberContribution>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<InterestCreditRecord> InterestCreditRecords => Set<InterestCreditRecord>();
    public DbSet<BenefitClaim> BenefitClaims => Set<BenefitClaim>();
    public DbSet<ClaimDisbursement> ClaimDisbursements => Set<ClaimDisbursement>();
    public DbSet<InvestmentPortfolio> InvestmentPortfolios => Set<InvestmentPortfolio>();
    public DbSet<CorpusRecord> CorpusRecords => Set<CorpusRecord>();
    public DbSet<AnnuityPlan> AnnuityPlans => Set<AnnuityPlan>();
    public DbSet<MonthlyPensionDisbursement> MonthlyPensionDisbursements => Set<MonthlyPensionDisbursement>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        });

        // ── AuditLog ──────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.AuditId);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.RecordId).HasMaxLength(100);
            e.HasOne(x => x.User).WithMany(u => u.AuditLogs)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── FundScheme ────────────────────────────────────────────────────
        modelBuilder.Entity<FundScheme>(e =>
        {
            e.HasKey(x => x.SchemeId);
            e.Property(x => x.SchemeName).HasMaxLength(150).IsRequired();
            e.Property(x => x.SchemeType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EmployeeContributionRate).HasPrecision(5, 2);
            e.Property(x => x.EmployerContributionRate).HasPrecision(5, 2);
            e.Property(x => x.InterestRatePA).HasPrecision(5, 2);
        });

        // ── Employer ──────────────────────────────────────────────────────
        modelBuilder.Entity<Employer>(e =>
        {
            e.HasKey(x => x.EmployerId);
            e.HasIndex(x => x.RegistrationNumber).IsUnique();
            e.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(x => x.RegistrationNumber).HasMaxLength(100);
            e.Property(x => x.Industry).HasMaxLength(100);
            e.Property(x => x.RemittanceFrequency).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ContactDetails).HasMaxLength(1000);
        });

        // ── Member ────────────────────────────────────────────────────────
        modelBuilder.Entity<Member>(e =>
        {
            e.HasKey(x => x.MemberId);
            e.HasIndex(x => x.MembershipNumber).IsUnique();
            e.Property(x => x.MembershipNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Gender).HasMaxLength(10);
            e.Property(x => x.NationalIdRef).HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.User).WithOne(u => u.Member)
                .HasForeignKey<Member>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Employer).WithMany(emp => emp.Members)
                .HasForeignKey(x => x.EmployerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── FundAccount ───────────────────────────────────────────────────
        modelBuilder.Entity<FundAccount>(e =>
        {
            e.HasKey(x => x.AccountId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EmployeeContributionBalance).HasPrecision(18, 2);
            e.Property(x => x.EmployerContributionBalance).HasPrecision(18, 2);
            e.Property(x => x.InterestAccrued).HasPrecision(18, 2);
            e.Property(x => x.TotalBalance).HasPrecision(18, 2);
            e.Property(x => x.VestingPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Member).WithMany(m => m.FundAccounts)
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Scheme).WithMany(s => s.FundAccounts)
                .HasForeignKey(x => x.SchemeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ContributionRemittance ────────────────────────────────────────
        modelBuilder.Entity<ContributionRemittance>(e =>
        {
            e.HasKey(x => x.RemittanceId);
            e.Property(x => x.RemittancePeriod).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalEmployeeShare).HasPrecision(18, 2);
            e.Property(x => x.TotalEmployerShare).HasPrecision(18, 2);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Employer).WithMany(emp => emp.Remittances)
                .HasForeignKey(x => x.EmployerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── MemberContribution ────────────────────────────────────────────
        modelBuilder.Entity<MemberContribution>(e =>
        {
            e.HasKey(x => x.ContributionId);
            e.Property(x => x.Period).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EmployeeAmount).HasPrecision(18, 2);
            e.Property(x => x.EmployerAmount).HasPrecision(18, 2);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Remittance).WithMany(r => r.MemberContributions)
                .HasForeignKey(x => x.RemittanceId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Member).WithMany(m => m.Contributions)
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── LedgerEntry ───────────────────────────────────────────────────
        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.EntryId);
            e.Property(x => x.EntryType).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ReferenceId).HasMaxLength(100);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            e.HasOne(x => x.Account).WithMany(a => a.LedgerEntries)
                .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── InterestCreditRecord ──────────────────────────────────────────
        modelBuilder.Entity<InterestCreditRecord>(e =>
        {
            e.HasKey(x => x.InterestId);
            e.Property(x => x.FinancialYear).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            e.Property(x => x.TotalContributions).HasPrecision(18, 2);
            e.Property(x => x.InterestRateApplied).HasPrecision(5, 2);
            e.Property(x => x.InterestAmount).HasPrecision(18, 2);
            e.Property(x => x.ClosingBalance).HasPrecision(18, 2);
            e.HasOne(x => x.Account).WithMany(a => a.InterestRecords)
                .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── BenefitClaim ──────────────────────────────────────────────────
        modelBuilder.Entity<BenefitClaim>(e =>
        {
            e.HasKey(x => x.ClaimId);
            e.Property(x => x.ClaimType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            e.Property(x => x.VestedAmount).HasPrecision(18, 2);
            e.Property(x => x.TaxDeductible).HasPrecision(18, 2);
            e.HasOne(x => x.Member).WithMany(m => m.Claims)
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProcessedBy).WithMany()
                .HasForeignKey(x => x.ProcessedById).OnDelete(DeleteBehavior.SetNull);
        });

        // ── ClaimDisbursement ─────────────────────────────────────────────
        modelBuilder.Entity<ClaimDisbursement>(e =>
        {
            e.HasKey(x => x.DisbursementId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.BankAccountRef).HasMaxLength(200);
            e.Property(x => x.DisbursedAmount).HasPrecision(18, 2);
            e.Property(x => x.TaxDeducted).HasPrecision(18, 2);
            e.Property(x => x.NetAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Claim).WithMany(c => c.Disbursements)
                .HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Member).WithMany()
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── InvestmentPortfolio ───────────────────────────────────────────
        modelBuilder.Entity<InvestmentPortfolio>(e =>
        {
            e.HasKey(x => x.PortfolioId);
            e.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.AllocationPercent).HasPrecision(5, 2);
            e.Property(x => x.InvestedValue).HasPrecision(18, 2);
            e.Property(x => x.CurrentValue).HasPrecision(18, 2);
            e.Property(x => x.YieldEarned).HasPrecision(18, 2);
            e.HasOne(x => x.Scheme).WithMany(s => s.Portfolios)
                .HasForeignKey(x => x.SchemeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── CorpusRecord ──────────────────────────────────────────────────
        modelBuilder.Entity<CorpusRecord>(e =>
        {
            e.HasKey(x => x.CorpusId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalContributions).HasPrecision(18, 2);
            e.Property(x => x.TotalWithdrawals).HasPrecision(18, 2);
            e.Property(x => x.InvestmentIncome).HasPrecision(18, 2);
            e.Property(x => x.ManagementExpenses).HasPrecision(18, 2);
            e.Property(x => x.ClosingCorpus).HasPrecision(18, 2);
            e.HasOne(x => x.Scheme).WithMany(s => s.CorpusRecords)
                .HasForeignKey(x => x.SchemeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AnnuityPlan ───────────────────────────────────────────────────
        modelBuilder.Entity<AnnuityPlan>(e =>
        {
            e.HasKey(x => x.AnnuityId);
            e.Property(x => x.PlanType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PurchaseValue).HasPrecision(18, 2);
            e.Property(x => x.MonthlyPension).HasPrecision(18, 2);
            e.HasOne(x => x.Member).WithMany(m => m.AnnuityPlans)
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── MonthlyPensionDisbursement ────────────────────────────────────
        modelBuilder.Entity<MonthlyPensionDisbursement>(e =>
        {
            e.HasKey(x => x.DisbursementId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.GrossAmount).HasPrecision(18, 2);
            e.Property(x => x.TaxDeducted).HasPrecision(18, 2);
            e.Property(x => x.NetAmount).HasPrecision(18, 2);
            e.HasOne(x => x.AnnuityPlan).WithMany(a => a.PensionDisbursements)
                .HasForeignKey(x => x.AnnuityId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Member).WithMany()
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Notification ──────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.User).WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
