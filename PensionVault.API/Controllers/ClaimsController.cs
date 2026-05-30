using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PensionVault.Application.DTOs.Claims;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize]
[Produces("application/json")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;
    public ClaimsController(IClaimService claimService) => _claimService = claimService;

    [HttpPost]
    [Authorize(Roles = "Member,FundAdmin")]
    public async Task<IActionResult> Submit([FromBody] CreateClaimRequest request)
    {
        var result = await _claimService.SubmitClaimAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ClaimId }, result);
    }

    [HttpGet]
    [Authorize(Roles = "FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetAll() => Ok(await _claimService.GetAllClaimsAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _claimService.GetClaimAsync(id));

    [HttpPut("{id:guid}/review")]
    [Authorize(Roles = "FundAdmin")]
    public async Task<IActionResult> Review(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _claimService.ReviewClaimAsync(id, userId));
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _claimService.ApproveClaimAsync(id, userId));
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _claimService.RejectClaimAsync(id, userId));
    }

    [HttpPost("{id:guid}/disburse")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Disburse(Guid id, [FromBody] DisburseClaimRequest request)
        => Ok(await _claimService.DisburseClaimAsync(id, request));
}
