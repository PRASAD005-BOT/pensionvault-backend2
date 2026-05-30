using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Application.DTOs.Members;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/members")]
[Authorize]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    public MembersController(IMemberService memberService) => _memberService = memberService;

    /// <summary>Get all members (FundAdmin only)</summary>
    [HttpGet]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> GetAll() => Ok(await _memberService.GetAllAsync());

    /// <summary>Get a specific member by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _memberService.GetByIdAsync(id));

    /// <summary>Enrol a new member</summary>
    [HttpPost]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var result = await _memberService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.MemberId }, result);
    }

    /// <summary>Update member details</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request)
        => Ok(await _memberService.UpdateAsync(id, request));

    /// <summary>Get a member's fund accounts</summary>
    [HttpGet("{id:guid}/fund-accounts")]
    public async Task<IActionResult> GetFundAccounts(Guid id)
        => Ok(await _memberService.GetFundAccountsAsync(id));

    /// <summary>Get a member's contribution history</summary>
    [HttpGet("{id:guid}/contributions")]
    public async Task<IActionResult> GetContributions(Guid id)
        => Ok(await _memberService.GetContributionsAsync(id));

    /// <summary>Get a member's ledger entries</summary>
    [HttpGet("{id:guid}/ledger")]
    public async Task<IActionResult> GetLedger(Guid id)
        => Ok(await _memberService.GetLedgerAsync(id));

    /// <summary>Get a member's benefit claims</summary>
    [HttpGet("{id:guid}/claims")]
    public async Task<IActionResult> GetClaims(Guid id)
        => Ok(await _memberService.GetClaimsAsync(id));
}
