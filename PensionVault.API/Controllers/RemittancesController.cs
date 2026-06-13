using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Application.DTOs.Contributions;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/remittances")]
[Authorize]
[Produces("application/json")]
public class RemittancesController : ControllerBase
{
    private readonly IContributionService _contributionService;
    public RemittancesController(IContributionService contributionService)
        => _contributionService = contributionService;

    /// <summary>Get all remittances</summary>
    [HttpGet]
    [Authorize(Roles = "Employer,FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetAll()
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId)) 
                return Ok(new List<RemittanceResponse>());
            return Ok(await _contributionService.GetEmployerRemittancesAsync(orgId));
        }
        return Ok(await _contributionService.GetAllRemittancesAsync());
    }

    /// <summary>Submit a contribution remittance for an employer</summary>
    [HttpPost]
    [Authorize(Roles = "Employer,FundAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateRemittanceRequest request)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim != null && Guid.TryParse(orgClaim.Value, out var orgId))
            {
                request = request with { EmployerId = orgId };
            }
        }
        var result = await _contributionService.CreateRemittanceAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.RemittanceId }, result);
    }

    /// <summary>Get remittance details</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employer,FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _contributionService.GetRemittanceAsync(id));

    /// <summary>Reconcile a remittance against enrolled headcount</summary>
    [HttpPost("{id:guid}/reconcile")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Reconcile(Guid id)
        => Ok(await _contributionService.ReconcileAsync(id));

    /// <summary>Get all contributions for a specific member</summary>
    [HttpGet("member/{memberId:guid}")]
    [Authorize(Roles = "Member,FundAdmin,Admin")]
    public async Task<IActionResult> GetMemberContributions(Guid memberId)
        => Ok(await _contributionService.GetMemberContributionsAsync(memberId));
}
