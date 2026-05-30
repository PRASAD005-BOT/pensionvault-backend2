using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Annuity;

public record CreateAnnuityRequest(
    Guid MemberId,
    AnnuityPlanType PlanType,
    decimal PurchaseValue,
    decimal MonthlyPension,
    DateTime AnnuityStartDate,
    string? NomineeDetails
);

public record AnnuityResponse(
    Guid AnnuityId,
    Guid MemberId,
    string MemberName,
    AnnuityPlanType PlanType,
    decimal PurchaseValue,
    decimal MonthlyPension,
    DateTime AnnuityStartDate,
    string? NomineeDetails,
    AnnuityStatus Status
);

public record ProcessDisbursementRequest(
    Guid AnnuityId,
    int Month,
    int Year,
    decimal TaxDeducted
);

public record PensionDisbursementResponse(
    Guid DisbursementId,
    Guid AnnuityId,
    Guid MemberId,
    string MemberName,
    int Month,
    int Year,
    decimal GrossAmount,
    decimal TaxDeducted,
    decimal NetAmount,
    DateTime? DisbursedDate,
    PensionDisbursementStatus Status
);
