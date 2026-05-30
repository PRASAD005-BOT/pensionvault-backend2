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
    [Authorize(Roles = "Compliance,Admin")]
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
}
