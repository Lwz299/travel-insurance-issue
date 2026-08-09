using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Authentication.Abstractions;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IOtpService
{
    string GenerateOtp();
    string HashOtp(string otp);
    bool VerifyOtp(string rawOtp, string otpHash);
}

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
    string GenerateRefreshToken();
    string GenerateResetToken(Guid userId, string email);
    (bool IsValid, Guid UserId, string Email) ValidateResetToken(string token);
}

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp, string purpose, CancellationToken cancellationToken = default);
}

public interface ITokenHasher
{
    string Hash(string value);
}
