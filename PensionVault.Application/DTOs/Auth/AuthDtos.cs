namespace PensionVault.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Name,
    string Email,
    string Password,
    string Role,
    string? Phone,
    Guid? OrganisationId
);

public record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    string Token,
    string RefreshToken,
    DateTime TokenExpiry
);

public record RefreshTokenRequest(string RefreshToken);
