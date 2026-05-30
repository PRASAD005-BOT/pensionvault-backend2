using PensionVault.Domain.Enums;

namespace PensionVault.Domain.Entities;

public class LedgerEntry
{
    public Guid EntryId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public EntryType EntryType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceId { get; set; }
    public LedgerEntryStatus Status { get; set; } = LedgerEntryStatus.Posted;

    // Navigation
    public FundAccount Account { get; set; } = null!;
}
