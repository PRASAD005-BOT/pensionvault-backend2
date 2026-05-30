using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Members;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Application.Services;

public interface IMemberService
{
    Task<IEnumerable<MemberResponse>> GetAllAsync();
    Task<MemberResponse> GetByIdAsync(Guid id);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request);
    Task<MemberResponse> UpdateAsync(Guid id, UpdateMemberRequest request);
    Task<IEnumerable<object>> GetFundAccountsAsync(Guid memberId);
    Task<IEnumerable<object>> GetContributionsAsync(Guid memberId);
    Task<IEnumerable<object>> GetLedgerAsync(Guid memberId);
    Task<IEnumerable<object>> GetClaimsAsync(Guid memberId);
}

public class MemberService : IMemberService
{
    private readonly IAppDbContext _context;
    public MemberService(IAppDbContext context) => _context = context;

    public async Task<IEnumerable<MemberResponse>> GetAllAsync()
    {
        return await _context.Members
            .Include(m => m.Employer)
            .Select(m => ToResponse(m))
            .ToListAsync();
    }

    public async Task<MemberResponse> GetByIdAsync(Guid id)
    {
        var member = await _context.Members.Include(m => m.Employer)
            .FirstOrDefaultAsync(m => m.MemberId == id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");
        return ToResponse(member);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request)
    {
        if (await _context.Members.AnyAsync(m => m.MembershipNumber == request.MembershipNumber))
            throw new InvalidOperationException("Membership number already exists.");

        var member = new Member
        {
            UserId = request.UserId,
            MembershipNumber = request.MembershipNumber,
            Name = request.Name,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            NationalIdRef = request.NationalIdRef,
            EmployerId = request.EmployerId,
            JoiningDate = request.JoiningDate,
            DateOfRetirement = request.DateOfRetirement,
            NomineeDetails = request.NomineeDetails,
            Status = MemberStatus.Active
        };
        _context.Members.Add(member);

        // Update employer count
        var employer = await _context.Employers.FindAsync(request.EmployerId);
        if (employer != null) employer.EnrolledMemberCount++;

        // Auto-create default Fund Account for the new member
        var defaultScheme = await _context.FundSchemes.FirstOrDefaultAsync();
        if (defaultScheme != null)
        {
            _context.FundAccounts.Add(new FundAccount
            {
                MemberId = member.MemberId,
                SchemeId = defaultScheme.SchemeId,
                AccountOpenDate = DateTime.UtcNow,
                VestingPercent = 100,
                Status = FundAccountStatus.Active
            });
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(member.MemberId);
    }

    public async Task<MemberResponse> UpdateAsync(Guid id, UpdateMemberRequest request)
    {
        var member = await _context.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");

        member.Name = request.Name;
        member.Gender = request.Gender;
        member.NationalIdRef = request.NationalIdRef;
        member.DateOfRetirement = request.DateOfRetirement;
        member.NomineeDetails = request.NomineeDetails;
        member.Status = request.Status;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<IEnumerable<object>> GetFundAccountsAsync(Guid memberId)
    {
        return await _context.FundAccounts
            .Include(a => a.Scheme)
            .Where(a => a.MemberId == memberId)
            .Select(a => (object)new
            {
                a.AccountId, a.MemberId, a.SchemeId,
                SchemeName = a.Scheme.SchemeName,
                a.AccountOpenDate, a.EmployeeContributionBalance,
                a.EmployerContributionBalance, a.InterestAccrued,
                a.TotalBalance, a.VestingPercent, Status = a.Status.ToString()
            }).ToListAsync();
    }

    public async Task<IEnumerable<object>> GetContributionsAsync(Guid memberId)
    {
        return await _context.MemberContributions
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.PostedDate)
            .Select(c => (object)new
            {
                c.ContributionId, c.Period, c.EmployeeAmount,
                c.EmployerAmount, c.TotalAmount, c.PostedDate,
                Status = c.Status.ToString()
            }).ToListAsync();
    }

    public async Task<IEnumerable<object>> GetLedgerAsync(Guid memberId)
    {
        var accountIds = await _context.FundAccounts
            .Where(a => a.MemberId == memberId)
            .Select(a => a.AccountId).ToListAsync();

        return await _context.LedgerEntries
            .Where(e => accountIds.Contains(e.AccountId))
            .OrderByDescending(e => e.EntryDate)
            .Select(e => (object)new
            {
                e.EntryId, e.AccountId, EntryType = e.EntryType.ToString(),
                e.Amount, e.BalanceAfter, e.EntryDate, e.ReferenceId,
                Status = e.Status.ToString()
            }).ToListAsync();
    }

    public async Task<IEnumerable<object>> GetClaimsAsync(Guid memberId)
    {
        return await _context.BenefitClaims
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.ClaimDate)
            .Select(c => (object)new
            {
                c.ClaimId, ClaimType = c.ClaimType.ToString(),
                c.ClaimDate, c.EligibleAmount, c.VestedAmount,
                c.TaxDeductible, Status = c.Status.ToString()
            }).ToListAsync();
    }

    private static MemberResponse ToResponse(Member m) => new(
        m.MemberId, m.MembershipNumber, m.Name, m.DateOfBirth,
        m.Gender, m.NationalIdRef, m.EmployerId,
        m.Employer?.CompanyName ?? "", m.JoiningDate,
        m.DateOfRetirement, m.NomineeDetails, m.Status);
}
