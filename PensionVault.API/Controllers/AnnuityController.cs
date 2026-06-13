using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Application.DTOs.Annuity;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/annuity")]
[Authorize]
[Produces("application/json")]
public class AnnuityController : ControllerBase
{
    private readonly IAnnuityService _annuityService;
    public AnnuityController(IAnnuityService annuityService) => _annuityService = annuityService;

    [HttpGet]
    [Authorize(Roles = "FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetAll()
        => Ok(await _annuityService.GetAllAnnuitiesAsync());

    [HttpPost]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAnnuityRequest request)
    {
        var result = await _annuityService.CreateAnnuityAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.AnnuityId }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _annuityService.GetAnnuityAsync(id));

    [HttpGet("{id:guid}/disbursements")]
    public async Task<IActionResult> GetDisbursements(Guid id)
        => Ok(await _annuityService.GetDisbursementsAsync(id));

    [HttpPost("{id:guid}/disburse")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> ProcessDisbursement(Guid id, [FromBody] ProcessDisbursementRequest request)
    {
        var req = request with { AnnuityId = id };
        return Ok(await _annuityService.ProcessDisbursementAsync(req));
    }
}
