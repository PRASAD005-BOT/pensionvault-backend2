using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Contributions;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Application.Services;

public interface IContributionService
{
    Task<RemittanceResponse> CreateRemittanceAsync(CreateRemittanceRequest request);
    Task<RemittanceResponse> GetRemittanceAsync(Guid remittanceId);
    Task<IEnumerable<RemittanceResponse>> GetEmployerRemittancesAsync(Guid employerId);
    Task<RemittanceResponse> ReconcileAsync(Guid remittanceId);
    Task<IEnumerable<MemberContributionResponse>> GetMemberContributionsAsync(Guid memberId);
}

public class ContributionService : IContributionService
{
    private readonly IAppDbContext _context;
    public ContributionService(IAppDbContext context) => _context = context;

    public async Task<RemittanceResponse> CreateRemittanceAsync(CreateRemittanceRequest request)
    {
        var employer = await _context.Employers.FindAsync(request.EmployerId)
            ?? throw new KeyNotFoundException("Employer not found.");

        var total = request.TotalEmployeeShare + request.TotalEmployerShare;
        var remittance = new ContributionRemittance
        {
            EmployerId = request.EmployerId,
            RemittancePeriod = request.RemittancePeriod,
            TotalEmployeeShare = request.TotalEmployeeShare,
            TotalEmployerShare = request.TotalEmployerShare,
            TotalAmount = total,
            RemittanceDate = DateTime.UtcNow,
            CoverageCount = request.CoverageCount,
            Status = RemittanceStatus.Received
        };
        _context.ContributionRemittances.Add(remittance);

        foreach (var item in request.MemberContributions)
        {
            var contribution = new MemberContribution
            {
                RemittanceId = remittance.RemittanceId,
                MemberId = item.MemberId,
                Period = request.RemittancePeriod,
                EmployeeAmount = item.EmployeeAmount,
                EmployerAmount = item.EmployerAmount,
                TotalAmount = item.EmployeeAmount + item.EmployerAmount,
                PostedDate = DateTime.UtcNow,
                Status = ContributionStatus.Posted
            };
            _context.MemberContributions.Add(contribution);

            // Post to ledger
            var account = await _context.FundAccounts
                .FirstOrDefaultAsync(a => a.MemberId == item.MemberId && a.Status == FundAccountStatus.Active);
            if (account != null)
            {
                account.EmployeeContributionBalance += item.EmployeeAmount;
                account.EmployerContributionBalance += item.EmployerAmount;
                account.TotalBalance += contribution.TotalAmount;

                _context.LedgerEntries.Add(new LedgerEntry
                {
                    AccountId = account.AccountId,
                    EntryType = EntryType.ContributionCredit,
                    Amount = contribution.TotalAmount,
                    BalanceAfter = account.TotalBalance,
                    ReferenceId = remittance.RemittanceId.ToString(),
                    Status = LedgerEntryStatus.Posted
                });
            }
        }

        await _context.SaveChangesAsync();
        return await GetRemittanceAsync(remittance.RemittanceId);
    }

    public async Task<RemittanceResponse> GetRemittanceAsync(Guid remittanceId)
    {
        var r = await _context.ContributionRemittances
            .Include(r => r.Employer)
            .FirstOrDefaultAsync(r => r.RemittanceId == remittanceId)
            ?? throw new KeyNotFoundException("Remittance not found.");
        return ToResponse(r);
    }

    public async Task<IEnumerable<RemittanceResponse>> GetEmployerRemittancesAsync(Guid employerId)
    {
        return await _context.ContributionRemittances
            .Include(r => r.Employer)
            .Where(r => r.EmployerId == employerId)
            .OrderByDescending(r => r.RemittanceDate)
            .Select(r => ToResponse(r))
            .ToListAsync();
    }

    public async Task<RemittanceResponse> ReconcileAsync(Guid remittanceId)
    {
        var remittance = await _context.ContributionRemittances.FindAsync(remittanceId)
            ?? throw new KeyNotFoundException("Remittance not found.");

        var postedCount = await _context.MemberContributions
            .CountAsync(c => c.RemittanceId == remittanceId && c.Status == ContributionStatus.Posted);

        remittance.Status = postedCount == remittance.CoverageCount
            ? RemittanceStatus.Reconciled
            : RemittanceStatus.Shortfall;

        await _context.SaveChangesAsync();
        return await GetRemittanceAsync(remittanceId);
    }

    public async Task<IEnumerable<MemberContributionResponse>> GetMemberContributionsAsync(Guid memberId)
    {
        return await _context.MemberContributions
            .Include(c => c.Member)
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.PostedDate)
            .Select(c => new MemberContributionResponse(
                c.ContributionId, c.MemberId, c.Member.Name,
                c.Period, c.EmployeeAmount, c.EmployerAmount,
                c.TotalAmount, c.PostedDate, c.Status))
            .ToListAsync();
    }

    private static RemittanceResponse ToResponse(ContributionRemittance r) => new(
        r.RemittanceId, r.EmployerId, r.Employer?.CompanyName ?? "",
        r.RemittancePeriod, r.TotalEmployeeShare, r.TotalEmployerShare,
        r.TotalAmount, r.RemittanceDate, r.CoverageCount, r.Status);
}
