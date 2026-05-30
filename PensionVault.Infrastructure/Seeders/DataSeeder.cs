using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;
using PensionVault.Infrastructure.Data;

namespace PensionVault.Infrastructure.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // ── Admin User ────────────────────────────────────────────────────
        var admin = new User
        {
            UserId = Guid.NewGuid(),
            Name = "System Administrator",
            Email = "admin@pensionvault.com",
            Role = UserRole.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Status = UserStatus.Active
        };
        context.Users.Add(admin);

        // ── Fund Schemes ──────────────────────────────────────────────────
        var epfScheme = new FundScheme
        {
            SchemeId = Guid.NewGuid(),
            SchemeName = "Employee Provident Fund",
            SchemeType = SchemeType.EPF,
            EmployeeContributionRate = 12.00m,
            EmployerContributionRate = 12.00m,
            InterestRatePA = 8.15m,
            VestingSchedule = "{\"years\": 5, \"percent\": 100}",
            Status = SchemeStatus.Active
        };
        var gratuityScheme = new FundScheme
        {
            SchemeId = Guid.NewGuid(),
            SchemeName = "Gratuity Trust Fund",
            SchemeType = SchemeType.Gratuity,
            EmployeeContributionRate = 0.00m,
            EmployerContributionRate = 4.81m,
            InterestRatePA = 7.50m,
            VestingSchedule = "{\"years\": 5, \"percent\": 100}",
            Status = SchemeStatus.Active
        };
        context.FundSchemes.AddRange(epfScheme, gratuityScheme);

        // ── Employer ──────────────────────────────────────────────────────
        var employer = new Employer
        {
            EmployerId = Guid.NewGuid(),
            CompanyName = "Acme Technologies Pvt Ltd",
            RegistrationNumber = "CIN-L99999MH2010PLC123456",
            Industry = "Information Technology",
            EnrolledMemberCount = 3,
            RemittanceFrequency = RemittanceFrequency.Monthly,
            ContactDetails = "{\"email\":\"hr@acmetech.com\",\"phone\":\"+91-22-12345678\"}",
            Status = EmployerStatus.Active
        };
        var employerUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Acme HR Manager",
            Email = "hr@acmetech.com",
            Role = UserRole.Employer,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employer@123"),
            OrganisationId = employer.EmployerId,
            Status = UserStatus.Active
        };
        context.Employers.Add(employer);
        context.Users.Add(employerUser);

        // ── Members ───────────────────────────────────────────────────────
        var memberUser1 = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Rajesh Kumar",
            Email = "rajesh.kumar@acmetech.com",
            Role = UserRole.Member,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
            OrganisationId = employer.EmployerId,
            Status = UserStatus.Active
        };
        var member1 = new Member
        {
            MemberId = Guid.NewGuid(),
            UserId = memberUser1.UserId,
            MembershipNumber = "PV-2024-00001",
            Name = "Rajesh Kumar",
            DateOfBirth = new DateTime(1985, 6, 15),
            Gender = "Male",
            NationalIdRef = "AAAPK1234C",
            EmployerId = employer.EmployerId,
            JoiningDate = new DateTime(2020, 1, 1),
            DateOfRetirement = new DateTime(2045, 6, 15),
            NomineeDetails = "{\"name\":\"Priya Kumar\",\"relation\":\"Spouse\",\"percent\":100}",
            Status = MemberStatus.Active
        };
        var memberUser2 = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Priya Sharma",
            Email = "priya.sharma@acmetech.com",
            Role = UserRole.Member,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
            OrganisationId = employer.EmployerId,
            Status = UserStatus.Active
        };
        var member2 = new Member
        {
            MemberId = Guid.NewGuid(),
            UserId = memberUser2.UserId,
            MembershipNumber = "PV-2024-00002",
            Name = "Priya Sharma",
            DateOfBirth = new DateTime(1990, 3, 22),
            Gender = "Female",
            NationalIdRef = "BBBPK5678D",
            EmployerId = employer.EmployerId,
            JoiningDate = new DateTime(2021, 4, 1),
            DateOfRetirement = new DateTime(2050, 3, 22),
            NomineeDetails = "{\"name\":\"Amit Sharma\",\"relation\":\"Spouse\",\"percent\":100}",
            Status = MemberStatus.Active
        };
        context.Users.AddRange(memberUser1, memberUser2);
        context.Members.AddRange(member1, member2);

        // ── Fund Accounts ─────────────────────────────────────────────────
        var account1 = new FundAccount
        {
            AccountId = Guid.NewGuid(),
            MemberId = member1.MemberId,
            SchemeId = epfScheme.SchemeId,
            AccountOpenDate = new DateTime(2020, 1, 1),
            EmployeeContributionBalance = 120000.00m,
            EmployerContributionBalance = 120000.00m,
            InterestAccrued = 32800.00m,
            TotalBalance = 272800.00m,
            VestingPercent = 100,
            Status = FundAccountStatus.Active
        };
        var account2 = new FundAccount
        {
            AccountId = Guid.NewGuid(),
            MemberId = member2.MemberId,
            SchemeId = epfScheme.SchemeId,
            AccountOpenDate = new DateTime(2021, 4, 1),
            EmployeeContributionBalance = 72000.00m,
            EmployerContributionBalance = 72000.00m,
            InterestAccrued = 15600.00m,
            TotalBalance = 159600.00m,
            VestingPercent = 100,
            Status = FundAccountStatus.Active
        };
        context.FundAccounts.AddRange(account1, account2);

        // ── Staff Users ───────────────────────────────────────────────────
        var fundAdmin = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Fund Administrator",
            Email = "fundadmin@pensionvault.com",
            Role = UserRole.FundAdmin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("FundAdmin@123"),
            Status = UserStatus.Active
        };
        var compliance = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Compliance Officer",
            Email = "compliance@pensionvault.com",
            Role = UserRole.Compliance,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Compliance@123"),
            Status = UserStatus.Active
        };
        var investOfficer = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Investment Officer",
            Email = "investment@pensionvault.com",
            Role = UserRole.InvestmentOfficer,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Invest@123"),
            Status = UserStatus.Active
        };
        context.Users.AddRange(fundAdmin, compliance, investOfficer);

        // ── Investment Portfolios ─────────────────────────────────────────
        context.InvestmentPortfolios.AddRange(
            new InvestmentPortfolio
            {
                PortfolioId = Guid.NewGuid(),
                SchemeId = epfScheme.SchemeId,
                AssetClass = AssetClass.GovernmentSecurities,
                AllocationPercent = 45.00m,
                InvestedValue = 5000000m,
                CurrentValue = 5250000m,
                YieldEarned = 250000m,
                LastUpdated = DateTime.UtcNow
            },
            new InvestmentPortfolio
            {
                PortfolioId = Guid.NewGuid(),
                SchemeId = epfScheme.SchemeId,
                AssetClass = AssetClass.CorporateBonds,
                AllocationPercent = 30.00m,
                InvestedValue = 3000000m,
                CurrentValue = 3150000m,
                YieldEarned = 150000m,
                LastUpdated = DateTime.UtcNow
            },
            new InvestmentPortfolio
            {
                PortfolioId = Guid.NewGuid(),
                SchemeId = epfScheme.SchemeId,
                AssetClass = AssetClass.Equity,
                AllocationPercent = 15.00m,
                InvestedValue = 1500000m,
                CurrentValue = 1650000m,
                YieldEarned = 150000m,
                LastUpdated = DateTime.UtcNow
            },
            new InvestmentPortfolio
            {
                PortfolioId = Guid.NewGuid(),
                SchemeId = epfScheme.SchemeId,
                AssetClass = AssetClass.FixedDeposit,
                AllocationPercent = 10.00m,
                InvestedValue = 1000000m,
                CurrentValue = 1080000m,
                YieldEarned = 80000m,
                LastUpdated = DateTime.UtcNow
            }
        );

        // ── Corpus Record ─────────────────────────────────────────────────
        context.CorpusRecords.Add(new CorpusRecord
        {
            CorpusId = Guid.NewGuid(),
            SchemeId = epfScheme.SchemeId,
            RecordDate = new DateTime(2024, 3, 31),
            TotalContributions = 10500000m,
            TotalWithdrawals = 500000m,
            InvestmentIncome = 630000m,
            ManagementExpenses = 50000m,
            ClosingCorpus = 10580000m,
            Status = CorpusStatus.Finalised
        });

        // ── Welcome Notifications ─────────────────────────────────────────
        context.Notifications.AddRange(
            new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = memberUser1.UserId,
                Message = "Welcome to PensionVault! Your EPF account has been created with balance ₹2,72,800.",
                Category = NotificationCategory.Contribution,
                Status = NotificationStatus.Unread,
                CreatedDate = DateTime.UtcNow
            },
            new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = memberUser2.UserId,
                Message = "Welcome to PensionVault! Your EPF account has been created with balance ₹1,59,600.",
                Category = NotificationCategory.Contribution,
                Status = NotificationStatus.Unread,
                CreatedDate = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();
    }
}
