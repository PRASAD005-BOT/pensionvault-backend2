using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Ledger;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Application.Services;

public interface ILedgerService
{
    Task<IEnumerable<LedgerEntryResponse>> GetAccountLedgerAsync(Guid accountId);
    Task<InterestCreditResponse> CreditInterestAsync(CreditInterestRequest request);
    Task<IEnumerable<InterestCreditResponse>> GetInterestRecordsAsync(Guid accountId);
}

public class LedgerService : ILedgerService
{
    private readonly IAppDbContext _context;
    public LedgerService(IAppDbContext context) => _context = context;

    public async Task<IEnumerable<LedgerEntryResponse>> GetAccountLedgerAsync(Guid accountId)
    {
        return await _context.LedgerEntries
            .Where(e => e.AccountId == accountId)
            .OrderByDescending(e => e.EntryDate)
            .Select(e => new LedgerEntryResponse(
                e.EntryId, e.AccountId, e.EntryType, e.Amount,
                e.BalanceAfter, e.EntryDate, e.ReferenceId, e.Status))
            .ToListAsync();
    }

    public async Task<InterestCreditResponse> CreditInterestAsync(CreditInterestRequest request)
    {
        var account = await _context.FundAccounts.FindAsync(request.AccountId)
            ?? throw new KeyNotFoundException("Fund account not found.");

        if (await _context.InterestCreditRecords.AnyAsync(r =>
                r.AccountId == request.AccountId && r.FinancialYear == request.FinancialYear))
            throw new InvalidOperationException($"Interest already credited for {request.FinancialYear}.");

        // Get total contributions this year from ledger
        var totalContributions = await _context.LedgerEntries
            .Where(e => e.AccountId == request.AccountId && e.EntryType == EntryType.ContributionCredit)
            .SumAsync(e => e.Amount);

        var openingBalance = account.TotalBalance - totalContributions;
        var interestAmount = Math.Round(
            (openingBalance + totalContributions / 2) * (request.InterestRate / 100), 2);

        var record = new InterestCreditRecord
        {
            AccountId = request.AccountId,
            FinancialYear = request.FinancialYear,
            OpeningBalance = openingBalance,
            TotalContributions = totalContributions,
            InterestRateApplied = request.InterestRate,
            InterestAmount = interestAmount,
            ClosingBalance = account.TotalBalance + interestAmount,
            CreditedDate = DateTime.UtcNow,
            Status = InterestCreditStatus.Credited
        };
        _context.InterestCreditRecords.Add(record);

        account.InterestAccrued += interestAmount;
        account.TotalBalance += interestAmount;

        _context.LedgerEntries.Add(new LedgerEntry
        {
            AccountId = account.AccountId,
            EntryType = EntryType.InterestCredit,
            Amount = interestAmount,
            BalanceAfter = account.TotalBalance,
            ReferenceId = record.InterestId.ToString(),
            Status = LedgerEntryStatus.Posted
        });

        await _context.SaveChangesAsync();
        return new InterestCreditResponse(
            record.InterestId, record.AccountId, record.FinancialYear,
            record.OpeningBalance, record.TotalContributions, record.InterestRateApplied,
            record.InterestAmount, record.ClosingBalance, record.CreditedDate, record.Status);
    }

    public async Task<IEnumerable<InterestCreditResponse>> GetInterestRecordsAsync(Guid accountId)
    {
        return await _context.InterestCreditRecords
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.FinancialYear)
            .Select(r => new InterestCreditResponse(
                r.InterestId, r.AccountId, r.FinancialYear,
                r.OpeningBalance, r.TotalContributions, r.InterestRateApplied,
                r.InterestAmount, r.ClosingBalance, r.CreditedDate, r.Status))
            .ToListAsync();
    }
}
