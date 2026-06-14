using System.Security.Claims;
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
    [Authorize(Roles = "Member,Employer,FundAdmin,Admin")]
    public async Task<IActionResult> GetAll()
    {
        if (User.IsInRole("Member"))
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();
            try {
                var member = await _memberService.GetByUserIdAsync(userId);
                return Ok(new List<MemberResponse> { member });
            } catch { return Ok(new List<MemberResponse>()); }
        }

        Guid? employerId = null;
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var parsedOrgId))
                return Ok(new List<MemberResponse>());
            employerId = parsedOrgId;
        }
        return Ok(await _memberService.GetAllAsync(employerId));
    }

    /// <summary>Get a specific member by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId) || member.EmployerId != orgId)
                return Forbid();
        }
        return Ok(member);
    }

    /// <summary>Get the authenticated member's profile</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Member,FundAdmin,Admin")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();
        return Ok(await _memberService.GetByUserIdAsync(userId));
    }

    /// <summary>Enrol a new member</summary>
    [HttpPost]
    [Authorize(Roles = "Employer,FundAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId)) return Forbid();
            request = request with { EmployerId = orgId };
        }
        var result = await _memberService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.MemberId }, result);
    }

    /// <summary>Update member details</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Member,Employer,FundAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request)
    {
        if (User.IsInRole("Member"))
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();
            var member = await _memberService.GetByUserIdAsync(userId);
            if (member.MemberId != id) return Forbid();
            request = request with { Status = member.Status };
        }
        else if (User.IsInRole("Employer"))
        {
            var member = await _memberService.GetByIdAsync(id);
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId) || member.EmployerId != orgId)
                return Forbid();
        }
        return Ok(await _memberService.UpdateAsync(id, request));
    }

    /// <summary>Self-enroll a member profile</summary>
    [HttpPost("self-enroll")]
    [Authorize(Roles = "Member")]
    public async Task<IActionResult> SelfEnroll([FromBody] SelfEnrollMemberRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();
        var result = await _memberService.SelfEnrollAsync(userId, request);
        return Ok(result);
    }

    /// <summary>Approve a pending member profile</summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Employer,FundAdmin,Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveMemberRequest request)
    {
        if (User.IsInRole("Employer"))
        {
            var member = await _memberService.GetByIdAsync(id);
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId) || member.EmployerId != orgId || request.EmployerId != orgId)
                return Forbid();
        }
        return Ok(await _memberService.ApproveAsync(id, request));
    }

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
