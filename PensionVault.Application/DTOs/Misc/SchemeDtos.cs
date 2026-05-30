using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Schemes;

public record CreateSchemeRequest(
    string SchemeName,
    SchemeType SchemeType,
    decimal EmployeeContributionRate,
    decimal EmployerContributionRate,
    decimal InterestRatePA,
    string? VestingSchedule
);

public record UpdateSchemeRequest(
    string SchemeName,
    decimal EmployeeContributionRate,
    decimal EmployerContributionRate,
    decimal InterestRatePA,
    string? VestingSchedule,
    SchemeStatus Status
);

public record SchemeResponse(
    Guid SchemeId,
    string SchemeName,
    SchemeType SchemeType,
    decimal EmployeeContributionRate,
    decimal EmployerContributionRate,
    decimal InterestRatePA,
    string? VestingSchedule,
    SchemeStatus Status
);
