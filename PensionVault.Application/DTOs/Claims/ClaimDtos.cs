using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Claims;

public record CreateClaimRequest(
    Guid MemberId,
    ClaimType ClaimType,
    decimal EligibleAmount
);

public record ClaimActionRequest(string? Remarks);

public record DisburseClaimRequest(
    decimal DisbursedAmount,
    decimal TaxDeducted,
    string BankAccountRef
);

public record ClaimResponse(
    Guid ClaimId,
    Guid MemberId,
    string MemberName,
    ClaimType ClaimType,
    DateTime ClaimDate,
    decimal EligibleAmount,
    decimal VestedAmount,
    decimal TaxDeductible,
    string? ProcessedByName,
    ClaimStatus Status
);

public record DisbursementResponse(
    Guid DisbursementId,
    Guid ClaimId,
    decimal DisbursedAmount,
    decimal TaxDeducted,
    decimal NetAmount,
    string? BankAccountRef,
    DateTime? DisbursedDate,
    DisbursementStatus Status
);
