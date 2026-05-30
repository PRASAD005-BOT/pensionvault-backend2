using PensionVault.Domain.Enums;

namespace PensionVault.Application.DTOs.Members;

public record CreateMemberRequest(
    Guid UserId,
    string MembershipNumber,
    string Name,
    DateTime DateOfBirth,
    string? Gender,
    string? NationalIdRef,
    Guid EmployerId,
    DateTime JoiningDate,
    DateTime? DateOfRetirement,
    string? NomineeDetails
);

public record UpdateMemberRequest(
    string Name,
    string? Gender,
    string? NationalIdRef,
    DateTime? DateOfRetirement,
    string? NomineeDetails,
    MemberStatus Status
);

public record MemberResponse(
    Guid MemberId,
    string MembershipNumber,
    string Name,
    DateTime DateOfBirth,
    string? Gender,
    string? NationalIdRef,
    Guid EmployerId,
    string EmployerName,
    DateTime JoiningDate,
    DateTime? DateOfRetirement,
    string? NomineeDetails,
    MemberStatus Status
);
