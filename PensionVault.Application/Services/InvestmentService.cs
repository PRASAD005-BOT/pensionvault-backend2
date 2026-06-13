using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Investment;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Application.Services;

public interface IInvestmentService
{
    Task<IEnumerable<PortfolioResponse>> GetPortfoliosAsync(Guid? schemeId = null);
    Task<PortfolioResponse> CreatePortfolioAsync(CreatePortfolioRequest request);
    Task<PortfolioResponse> UpdatePortfolioAsync(Guid portfolioId, UpdatePortfolioRequest request);
    Task<IEnumerable<CorpusResponse>> GetCorpusRecordsAsync(Guid? schemeId = null);
    Task<CorpusResponse> CreateCorpusRecordAsync(CreateCorpusRequest request);
    Task<CorpusResponse> FinaliseCorpusAsync(Guid corpusId);
}

public class InvestmentService : IInvestmentService
{
    private readonly IAppDbContext _context;
    public InvestmentService(IAppDbContext context) => _context = context;

    public async Task<IEnumerable<PortfolioResponse>> GetPortfoliosAsync(Guid? schemeId = null)
    {
        var query = _context.InvestmentPortfolios.Include(p => p.Scheme).AsQueryable();
        if (schemeId.HasValue) query = query.Where(p => p.SchemeId == schemeId);
        return await query.Select(p => ToPortfolioResponse(p)).ToListAsync();
    }

    public async Task<PortfolioResponse> CreatePortfolioAsync(CreatePortfolioRequest request)
    {
        var portfolio = new InvestmentPortfolio
        {
            SchemeId = request.SchemeId,
            AssetClass = request.AssetClass,
            AllocationPercent = request.AllocationPercent,
            InvestedValue = request.InvestedValue,
            CurrentValue = request.CurrentValue,
            YieldEarned = request.YieldEarned,
            LastUpdated = DateTime.UtcNow
        };
        _context.InvestmentPortfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var created = await _context.InvestmentPortfolios.Include(p => p.Scheme)
            .FirstAsync(p => p.PortfolioId == portfolio.PortfolioId);
        return ToPortfolioResponse(created);
    }

    public async Task<PortfolioResponse> UpdatePortfolioAsync(Guid portfolioId, UpdatePortfolioRequest request)
    {
        var portfolio = await _context.InvestmentPortfolios.Include(p => p.Scheme)
            .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId)
            ?? throw new KeyNotFoundException("Portfolio not found.");

        portfolio.AllocationPercent = request.AllocationPercent;
        portfolio.InvestedValue = request.InvestedValue;
        portfolio.CurrentValue = request.CurrentValue;
        portfolio.YieldEarned = request.YieldEarned;
        portfolio.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToPortfolioResponse(portfolio);
    }

    public async Task<IEnumerable<CorpusResponse>> GetCorpusRecordsAsync(Guid? schemeId = null)
    {
        var query = _context.CorpusRecords.Include(c => c.Scheme).AsQueryable();
        if (schemeId.HasValue) query = query.Where(c => c.SchemeId == schemeId);
        return await query.OrderByDescending(c => c.RecordDate)
            .Select(c => ToCorpusResponse(c)).ToListAsync();
    }

    public async Task<CorpusResponse> CreateCorpusRecordAsync(CreateCorpusRequest request)
    {
        var lastCorpus = await _context.CorpusRecords
            .Where(c => c.SchemeId == request.SchemeId && c.Status == CorpusStatus.Finalised)
            .OrderByDescending(c => c.RecordDate)
            .FirstOrDefaultAsync();

        var openingCorpus = lastCorpus?.ClosingCorpus ?? 0;

        var corpus = new CorpusRecord
        {
            SchemeId = request.SchemeId,
            RecordDate = request.RecordDate,
            TotalContributions = request.TotalContributions,
            TotalWithdrawals = request.TotalWithdrawals,
            InvestmentIncome = request.InvestmentIncome,
            ManagementExpenses = request.ManagementExpenses,
            ClosingCorpus = openingCorpus + request.TotalContributions - request.TotalWithdrawals
                + request.InvestmentIncome - request.ManagementExpenses,
            Status = CorpusStatus.Draft
        };
        _context.CorpusRecords.Add(corpus);
        await _context.SaveChangesAsync();

        var created = await _context.CorpusRecords.Include(c => c.Scheme)
            .FirstAsync(c => c.CorpusId == corpus.CorpusId);
        return ToCorpusResponse(created);
    }

    public async Task<CorpusResponse> FinaliseCorpusAsync(Guid corpusId)
    {
        var corpus = await _context.CorpusRecords.Include(c => c.Scheme)
            .FirstOrDefaultAsync(c => c.CorpusId == corpusId)
            ?? throw new KeyNotFoundException("Corpus record not found.");
        corpus.Status = CorpusStatus.Finalised;
        await _context.SaveChangesAsync();
        return ToCorpusResponse(corpus);
    }

    private static PortfolioResponse ToPortfolioResponse(InvestmentPortfolio p) => new(
        p.PortfolioId, p.SchemeId, p.Scheme?.SchemeName ?? "",
        p.AssetClass, p.AllocationPercent, p.InvestedValue,
        p.CurrentValue, p.YieldEarned, p.LastUpdated);

    private static CorpusResponse ToCorpusResponse(CorpusRecord c) => new(
        c.CorpusId, c.SchemeId, c.Scheme?.SchemeName ?? "",
        c.RecordDate, 
        c.ClosingCorpus - c.TotalContributions + c.TotalWithdrawals - c.InvestmentIncome + c.ManagementExpenses,
        c.TotalContributions, c.TotalWithdrawals,
        c.InvestmentIncome, c.ManagementExpenses, c.ClosingCorpus, c.Status);
}
