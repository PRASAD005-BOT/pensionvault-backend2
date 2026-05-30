using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Application.DTOs.Investment;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/portfolios")]
[Authorize(Roles = "InvestmentOfficer,FundAdmin,Admin,Compliance")]
[Produces("application/json")]
public class PortfoliosController : ControllerBase
{
    private readonly IInvestmentService _investmentService;
    public PortfoliosController(IInvestmentService investmentService)
        => _investmentService = investmentService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? schemeId)
        => Ok(await _investmentService.GetPortfoliosAsync(schemeId));

    [HttpPost]
    [Authorize(Roles = "InvestmentOfficer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioRequest request)
    {
        var result = await _investmentService.CreatePortfolioAsync(request);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "InvestmentOfficer,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePortfolioRequest request)
        => Ok(await _investmentService.UpdatePortfolioAsync(id, request));
}

[ApiController]
[Route("api/corpus")]
[Authorize(Roles = "InvestmentOfficer,FundAdmin,Admin,Compliance")]
[Produces("application/json")]
public class CorpusController : ControllerBase
{
    private readonly IInvestmentService _investmentService;
    public CorpusController(IInvestmentService investmentService)
        => _investmentService = investmentService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? schemeId)
        => Ok(await _investmentService.GetCorpusRecordsAsync(schemeId));

    [HttpPost]
    [Authorize(Roles = "InvestmentOfficer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCorpusRequest request)
    {
        var result = await _investmentService.CreateCorpusRecordAsync(request);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPost("{id:guid}/finalise")]
    [Authorize(Roles = "InvestmentOfficer,Admin")]
    public async Task<IActionResult> Finalise(Guid id)
        => Ok(await _investmentService.FinaliseCorpusAsync(id));
}
