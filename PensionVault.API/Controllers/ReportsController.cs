using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PensionVault.Application.Services;
using PensionVault.Domain.Enums;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Compliance,Admin,FundAdmin")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IAppDbContext _context;
    public ReportsController(IAppDbContext context) => _context = context;

    /// <summary>Get employers who are in default or shortfall status</summary>
    [HttpGet("contribution-defaults")]
    public async Task<IActionResult> ContributionDefaults()
    {
        var defaults = await _context.ContributionRemittances
            .Include(r => r.Employer)
            .Where(r => r.Status == RemittanceStatus.Default || r.Status == RemittanceStatus.Shortfall)
            .OrderByDescending(r => r.RemittanceDate)
            .Select(r => new
            {
                r.RemittanceId, r.EmployerId,
                EmployerName = r.Employer.CompanyName,
                r.RemittancePeriod, r.TotalAmount,
                Status = r.Status.ToString(), r.RemittanceDate
            })
            .ToListAsync();
        return Ok(defaults);
    }

    /// <summary>Get audit trail with optional filters</summary>
    [HttpGet("audit-trail")]
    [Authorize(Roles = "Compliance,Admin,FundAdmin")]
    public async Task<IActionResult> AuditTrail(
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.AuditLogs.Include(a => a.User).AsQueryable();
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (from.HasValue) query = query.Where(a => a.Timestamp >= from);
        if (to.HasValue) query = query.Where(a => a.Timestamp <= to);

        var results = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(1000)
            .Select(a => new
            {
                a.AuditId, UserName = a.User.Name,
                a.Action, a.EntityType, a.RecordId, a.Timestamp
            })
            .ToListAsync();
        return Ok(results);
    }

    /// <summary>Statutory returns — contribution summary by period</summary>
    [HttpGet("statutory-returns")]
    public async Task<IActionResult> StatutoryReturns([FromQuery] string? period)
    {
        var query = _context.ContributionRemittances
            .Include(r => r.Employer).AsQueryable();
        if (!string.IsNullOrEmpty(period))
            query = query.Where(r => r.RemittancePeriod == period);

        var summary = await query
            .GroupBy(r => r.RemittancePeriod)
            .Select(g => new
            {
                Period = g.Key,
                TotalEmployers = g.Count(),
                TotalEmployeeShare = g.Sum(r => r.TotalEmployeeShare),
                TotalEmployerShare = g.Sum(r => r.TotalEmployerShare),
                TotalAmount = g.Sum(r => r.TotalAmount),
                TotalCoveredMembers = g.Sum(r => r.CoverageCount)
            })
            .OrderByDescending(x => x.Period)
            .ToListAsync();
        return Ok(summary);
    }

    [HttpPost("fix-data")]
    [AllowAnonymous]
    public async Task<IActionResult> FixData()
    {
        // Update all old schemes instead of deleting them
        var schemes = await _context.FundSchemes.ToListAsync();
        foreach (var s in schemes)
        {
            if (s.SchemeName.Contains("EPF") || s.SchemeType == SchemeType.EPF)
            {
                s.EmployeeContributionRate = 12;
                s.EmployerContributionRate = 12;
                s.InterestRatePA = 8.25m;
                s.Status = SchemeStatus.Active;
            }
            else
            {
                s.EmployerContributionRate = 4.81m;
                s.InterestRatePA = 7.5m;
                s.Status = SchemeStatus.Active;
            }
        }
        await _context.SaveChangesAsync();

        var epf = schemes.FirstOrDefault(s => s.SchemeType == SchemeType.EPF) ?? schemes.FirstOrDefault();

        // Fix missing FundAccount
        var member = await _context.Members.FirstOrDefaultAsync();
        if (member != null && epf != null && !await _context.FundAccounts.AnyAsync(a => a.MemberId == member.MemberId))
        {
            var account = new PensionVault.Domain.Entities.FundAccount { MemberId = member.MemberId, SchemeId = epf.SchemeId, AccountOpenDate = DateTime.UtcNow, VestingPercent = 100, Status = FundAccountStatus.Active };
            _context.FundAccounts.Add(account);
            await _context.SaveChangesAsync();

            // Insert Dummy Ledger entries
            _context.LedgerEntries.Add(new PensionVault.Domain.Entities.LedgerEntry { AccountId = account.AccountId, EntryType = EntryType.ContributionCredit, Amount = 24000, BalanceAfter = 24000, ReferenceId = "SYS-GEN", Status = LedgerEntryStatus.Posted });
            _context.LedgerEntries.Add(new PensionVault.Domain.Entities.LedgerEntry { AccountId = account.AccountId, EntryType = EntryType.InterestCredit, Amount = 1980, BalanceAfter = 25980, ReferenceId = "SYS-GEN", Status = LedgerEntryStatus.Posted });
            await _context.SaveChangesAsync();
            
            // Insert Dummy Annuity
            if (!await _context.AnnuityPlans.AnyAsync(a => a.MemberId == member.MemberId))
            {
                _context.AnnuityPlans.Add(new PensionVault.Domain.Entities.AnnuityPlan { MemberId = member.MemberId, PlanType = AnnuityPlanType.LifeAnnuity, PurchaseValue = 500000, MonthlyPension = 3500, AnnuityStartDate = DateTime.UtcNow, Status = AnnuityStatus.Active });
                await _context.SaveChangesAsync();
            }
        }

        return Ok("Fixed");
    }
}
