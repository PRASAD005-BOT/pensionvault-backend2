using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Claims;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Application.Services;

public interface IClaimService
{
    Task<ClaimResponse> SubmitClaimAsync(CreateClaimRequest request);
    Task<IEnumerable<ClaimResponse>> GetAllClaimsAsync();
    Task<ClaimResponse> GetClaimAsync(Guid claimId);
    Task<ClaimResponse> ReviewClaimAsync(Guid claimId, Guid processedById);
    Task<ClaimResponse> ApproveClaimAsync(Guid claimId, Guid processedById);
    Task<ClaimResponse> RejectClaimAsync(Guid claimId, Guid processedById);
    Task<DisbursementResponse> DisburseClaimAsync(Guid claimId, DisburseClaimRequest request);
}

public class ClaimService : IClaimService
{
    private readonly IAppDbContext _context;
    public ClaimService(IAppDbContext context) => _context = context;

    public async Task<ClaimResponse> SubmitClaimAsync(CreateClaimRequest request)
    {
        var member = await _context.Members.FindAsync(request.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        var account = await _context.FundAccounts
            .FirstOrDefaultAsync(a => a.MemberId == request.MemberId && a.Status == FundAccountStatus.Active);

        var vestedAmount = account != null
            ? Math.Round(account.TotalBalance * (account.VestingPercent / 100), 2)
            : 0;

        var claim = new BenefitClaim
        {
            MemberId = request.MemberId,
            ClaimType = request.ClaimType,
            ClaimDate = DateTime.UtcNow,
            EligibleAmount = request.EligibleAmount,
            VestedAmount = vestedAmount,
            TaxDeductible = Math.Round(request.EligibleAmount * 0.10m, 2), // 10% TDS
            Status = ClaimStatus.Submitted
        };
        _context.BenefitClaims.Add(claim);
        await _context.SaveChangesAsync();
        return await GetClaimAsync(claim.ClaimId);
    }

    public async Task<IEnumerable<ClaimResponse>> GetAllClaimsAsync()
    {
        return await _context.BenefitClaims
            .Include(c => c.Member)
            .Include(c => c.ProcessedBy)
            .OrderByDescending(c => c.ClaimDate)
            .Select(c => ToResponse(c))
            .ToListAsync();
    }

    public async Task<ClaimResponse> GetClaimAsync(Guid claimId)
    {
        var claim = await _context.BenefitClaims
            .Include(c => c.Member)
            .Include(c => c.ProcessedBy)
            .FirstOrDefaultAsync(c => c.ClaimId == claimId)
            ?? throw new KeyNotFoundException("Claim not found.");
        return ToResponse(claim);
    }

    public async Task<ClaimResponse> ReviewClaimAsync(Guid claimId, Guid processedById)
        => await UpdateStatusAsync(claimId, ClaimStatus.UnderReview, processedById);

    public async Task<ClaimResponse> ApproveClaimAsync(Guid claimId, Guid processedById)
        => await UpdateStatusAsync(claimId, ClaimStatus.Approved, processedById);

    public async Task<ClaimResponse> RejectClaimAsync(Guid claimId, Guid processedById)
        => await UpdateStatusAsync(claimId, ClaimStatus.Rejected, processedById);

    public async Task<DisbursementResponse> DisburseClaimAsync(Guid claimId, DisburseClaimRequest request)
    {
        var claim = await _context.BenefitClaims.FindAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");
        if (claim.Status != ClaimStatus.Approved)
            throw new InvalidOperationException("Claim must be approved before disbursement.");

        var disbursement = new ClaimDisbursement
        {
            ClaimId = claimId,
            MemberId = claim.MemberId,
            DisbursedAmount = request.DisbursedAmount,
            TaxDeducted = request.TaxDeducted,
            NetAmount = request.DisbursedAmount - request.TaxDeducted,
            BankAccountRef = request.BankAccountRef,
            DisbursedDate = DateTime.UtcNow,
            Status = DisbursementStatus.Processed
        };
        _context.ClaimDisbursements.Add(disbursement);
        claim.Status = ClaimStatus.Disbursed;

        // Debit ledger
        var account = await _context.FundAccounts
            .FirstOrDefaultAsync(a => a.MemberId == claim.MemberId && a.Status == FundAccountStatus.Active);
        if (account != null)
        {
            account.TotalBalance -= disbursement.NetAmount;
            _context.LedgerEntries.Add(new LedgerEntry
            {
                AccountId = account.AccountId,
                EntryType = EntryType.ClaimDebit,
                Amount = disbursement.NetAmount,
                BalanceAfter = account.TotalBalance,
                ReferenceId = claimId.ToString(),
                Status = LedgerEntryStatus.Posted
            });
        }

        await _context.SaveChangesAsync();
        return new DisbursementResponse(
            disbursement.DisbursementId, disbursement.ClaimId,
            disbursement.DisbursedAmount, disbursement.TaxDeducted,
            disbursement.NetAmount, disbursement.BankAccountRef,
            disbursement.DisbursedDate, disbursement.Status);
    }

    private async Task<ClaimResponse> UpdateStatusAsync(Guid claimId, ClaimStatus status, Guid processedById)
    {
        var claim = await _context.BenefitClaims.FindAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");
        claim.Status = status;
        claim.ProcessedById = processedById;
        await _context.SaveChangesAsync();
        return await GetClaimAsync(claimId);
    }

    private static ClaimResponse ToResponse(BenefitClaim c) => new(
        c.ClaimId, c.MemberId, c.Member?.Name ?? "",
        c.ClaimType, c.ClaimDate, c.EligibleAmount,
        c.VestedAmount, c.TaxDeductible,
        c.ProcessedBy?.Name, c.Status);
}
