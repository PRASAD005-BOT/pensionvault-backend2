using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Application.DTOs.Ledger;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/ledger")]
[Authorize]
[Produces("application/json")]
public class LedgerController : ControllerBase
{
    private readonly ILedgerService _ledgerService;
    public LedgerController(ILedgerService ledgerService) => _ledgerService = ledgerService;

    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetLedger(Guid accountId)
        => Ok(await _ledgerService.GetAccountLedgerAsync(accountId));

    [HttpPost("interest-credit")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> CreditInterest([FromBody] CreditInterestRequest request)
        => Ok(await _ledgerService.CreditInterestAsync(request));

    [HttpGet("interest-records/{accountId:guid}")]
    public async Task<IActionResult> GetInterestRecords(Guid accountId)
        => Ok(await _ledgerService.GetInterestRecordsAsync(accountId));
}
