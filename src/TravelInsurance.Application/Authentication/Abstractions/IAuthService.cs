using TravelInsurance.Application.Authentication.DTOs;

namespace TravelInsurance.Application.Authentication.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> VerifyEmailAsync(VerifyEmailRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> ResendOtpAsync(ResendOtpRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponse> VerifyResetOtpAsync(VerifyResetOtpRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> ResetPasswordAsync(ResetPasswordRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> LogoutAsync(LogoutRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OperationResponse> RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<OperationResponse> RevokeAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
