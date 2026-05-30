using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Schemes;
using PensionVault.Application.Services;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/schemes")]
[Authorize]
[Produces("application/json")]
public class SchemesController : ControllerBase
{
    private readonly IAppDbContext _context;
    public SchemesController(IAppDbContext context) => _context = context;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var schemes = await _context.FundSchemes
            .Select(s => new SchemeResponse(s.SchemeId, s.SchemeName, s.SchemeType,
                s.EmployeeContributionRate, s.EmployerContributionRate,
                s.InterestRatePA, s.VestingSchedule, s.Status))
            .ToListAsync();
        return Ok(schemes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var s = await _context.FundSchemes.FindAsync(id);
        if (s == null) return NotFound();
        return Ok(new SchemeResponse(s.SchemeId, s.SchemeName, s.SchemeType,
            s.EmployeeContributionRate, s.EmployerContributionRate,
            s.InterestRatePA, s.VestingSchedule, s.Status));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSchemeRequest request)
    {
        var scheme = new FundScheme
        {
            SchemeName = request.SchemeName,
            SchemeType = request.SchemeType,
            EmployeeContributionRate = request.EmployeeContributionRate,
            EmployerContributionRate = request.EmployerContributionRate,
            InterestRatePA = request.InterestRatePA,
            VestingSchedule = request.VestingSchedule,
            Status = SchemeStatus.Active
        };
        _context.FundSchemes.Add(scheme);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = scheme.SchemeId },
            new SchemeResponse(scheme.SchemeId, scheme.SchemeName, scheme.SchemeType,
                scheme.EmployeeContributionRate, scheme.EmployerContributionRate,
                scheme.InterestRatePA, scheme.VestingSchedule, scheme.Status));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchemeRequest request)
    {
        var scheme = await _context.FundSchemes.FindAsync(id);
        if (scheme == null) return NotFound();
        scheme.SchemeName = request.SchemeName;
        scheme.EmployeeContributionRate = request.EmployeeContributionRate;
        scheme.EmployerContributionRate = request.EmployerContributionRate;
        scheme.InterestRatePA = request.InterestRatePA;
        scheme.VestingSchedule = request.VestingSchedule;
        scheme.Status = request.Status;
        await _context.SaveChangesAsync();
        return Ok(new SchemeResponse(scheme.SchemeId, scheme.SchemeName, scheme.SchemeType,
            scheme.EmployeeContributionRate, scheme.EmployerContributionRate,
            scheme.InterestRatePA, scheme.VestingSchedule, scheme.Status));
    }
}
