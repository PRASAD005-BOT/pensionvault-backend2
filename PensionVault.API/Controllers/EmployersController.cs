using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Application.DTOs.Employers;
using PensionVault.Application.DTOs.Contributions;
using PensionVault.Application.Services;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/employers")]
[Authorize]
[Produces("application/json")]
public class EmployersController : ControllerBase
{
    private readonly IEmployerService _employerService;
    private readonly IContributionService _contributionService;
    public EmployersController(IEmployerService employerService, IContributionService contributionService)
    {
        _employerService = employerService;
        _contributionService = contributionService;
    }

    [HttpGet]
    [Authorize(Roles = "FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetAll() => Ok(await _employerService.GetAllAsync());

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employer,FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _employerService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployerRequest request)
    {
        var result = await _employerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.EmployerId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployerRequest request)
        => Ok(await _employerService.UpdateAsync(id, request));

    [HttpGet("{id:guid}/remittances")]
    [Authorize(Roles = "Employer,FundAdmin,Admin")]
    public async Task<IActionResult> GetRemittances(Guid id)
        => Ok(await _contributionService.GetEmployerRemittancesAsync(id));
}
