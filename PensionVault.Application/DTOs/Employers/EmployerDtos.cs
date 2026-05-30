using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Employers;

public record CreateEmployerRequest(
    string CompanyName,
    string RegistrationNumber,
    string? Industry,
    RemittanceFrequency RemittanceFrequency,
    string? ContactDetails
);

public record UpdateEmployerRequest(
    string CompanyName,
    string? Industry,
    RemittanceFrequency RemittanceFrequency,
    string? ContactDetails,
    EmployerStatus Status
);

public record EmployerResponse(
    Guid EmployerId,
    string CompanyName,
    string RegistrationNumber,
    string? Industry,
    int EnrolledMemberCount,
    RemittanceFrequency RemittanceFrequency,
    string? ContactDetails,
    EmployerStatus Status
);
