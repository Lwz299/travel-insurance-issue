using System.ComponentModel.DataAnnotations;

namespace TravelInsurance.Application.Authentication.DTOs;

public class RegisterRequest
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }
}

public class VerifyEmailRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, Length(6, 6)]
    public string Otp { get; set; } = string.Empty;
}

public class ResendOtpRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Device { get; set; }
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class VerifyResetOtpRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, Length(6, 6)]
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Device { get; set; }
}

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RevokeSessionRequest
{
    [Required]
    public Guid SessionId { get; set; }
}

public record UserInfoDto(Guid Id, string FullName, string Email, string? Phone, IReadOnlyCollection<string> Roles);

public class AuthResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public UserInfoDto? UserInfo { get; init; }
}

public record SessionDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, string? Device, string? IpAddress, string? UserAgent);

public class OperationResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
