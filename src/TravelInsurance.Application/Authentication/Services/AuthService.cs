using Microsoft.Extensions.Options;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Common.Options;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Authentication.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IOtpRepository otpRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordService passwordService,
    IOtpService otpService,
    IJwtService jwtService,
    IEmailService emailService,
    ITokenHasher tokenHasher,
    IOptions<SecurityOptions> securityOptions) : IAuthService
{
    private readonly SecurityOptions _security = securityOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var existing = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            return new AuthResponse { Success = false, Message = "Email already exists." };
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone?.Trim(),
            PasswordHash = passwordService.Hash(request.Password),
            EmailVerified = false,
            Status = UserStatus.Inactive
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.AddUserRoleAsync(user.Id, 2, cancellationToken);
        await CreateAndSendOtpAsync(user, OtpPurpose.Register, cancellationToken);

        await WriteAuditAsync(user.Id, "AUTH.REGISTER", "users", user.Id, ipAddress, userAgent, cancellationToken);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration succeeded. Please verify your email with OTP.",
            UserId = user.Id
        };
    }

    public async Task<OperationResponse> VerifyEmailAsync(VerifyEmailRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var otp = await otpRepository.GetLatestActiveOtpAsync(email, OtpPurpose.Register, cancellationToken);
        if (otp is null)
        {
            return new OperationResponse { Success = false, Message = "OTP not found." };
        }

        var validation = await ValidateOtpAsync(otp, request.Otp, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return new OperationResponse { Success = false, Message = "User not found." };
        }

        user.EmailVerified = true;
        user.Status = UserStatus.Active;
        await userRepository.UpdateAsync(user, cancellationToken);

        await WriteAuditAsync(user.Id, "AUTH.EMAIL_VERIFIED", "users", user.Id, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Email verified successfully." };
    }

    public async Task<OperationResponse> ResendOtpAsync(ResendOtpRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is not null && !user.EmailVerified)
        {
            await CreateAndSendOtpAsync(user, OtpPurpose.Register, cancellationToken);
            await WriteAuditAsync(user.Id, "AUTH.OTP_RESENT", "users", user.Id, ipAddress, userAgent, cancellationToken);
        }

        return new OperationResponse { Success = true, Message = "If the email exists and is not verified, a new OTP has been sent." };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return new AuthResponse { Success = false, Message = "Invalid credentials." };
        }

        if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTimeOffset.UtcNow)
        {
            return new AuthResponse { Success = false, Message = "Account is temporarily locked. Try again later." };
        }

        if (!passwordService.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= _security.MaxFailedLoginAttempts)
            {
                user.LockoutUntil = DateTimeOffset.UtcNow.AddMinutes(_security.LockoutMinutes);
                user.Status = UserStatus.Locked;
            }

            await userRepository.UpdateAsync(user, cancellationToken);
            await WriteAuditAsync(user.Id, "AUTH.LOGIN_FAILED", "users", user.Id, ipAddress, userAgent, cancellationToken);
            return new AuthResponse { Success = false, Message = "Invalid credentials." };
        }

        if (!user.EmailVerified)
        {
            return new AuthResponse { Success = false, Message = "Email is not verified." };
        }

        if (user.Status == UserStatus.Inactive)
        {
            return new AuthResponse { Success = false, Message = "Account is inactive." };
        }

        user.FailedLoginAttempts = 0;
        user.LockoutUntil = null;
        user.Status = UserStatus.Active;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userRepository.UpdateAsync(user, cancellationToken);

        var roles = user.UserRoles.Select(x => x.Role.Name).Distinct().ToArray();
        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToArray();

        var (accessToken, expiresAt) = jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = jwtService.GenerateRefreshToken();

        await refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHasher.Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_security.RefreshTokenDays),
            Device = request.Device,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);

        await WriteAuditAsync(user.Id, "AUTH.LOGIN_SUCCESS", "users", user.Id, ipAddress, userAgent, cancellationToken);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserInfo = new UserInfoDto(user.Id, user.FullName, user.Email, user.Phone, roles)
        };
    }

    public async Task<OperationResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is not null)
        {
            await CreateAndSendOtpAsync(user, OtpPurpose.ResetPassword, cancellationToken);
            await WriteAuditAsync(user.Id, "AUTH.FORGOT_PASSWORD", "users", user.Id, ipAddress, userAgent, cancellationToken);
        }

        return new OperationResponse { Success = true, Message = "If the email exists, an OTP has been sent." };
    }

    public async Task<AuthResponse> VerifyResetOtpAsync(VerifyResetOtpRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var otp = await otpRepository.GetLatestActiveOtpAsync(email, OtpPurpose.ResetPassword, cancellationToken);
        if (otp is null)
        {
            return new AuthResponse { Success = false, Message = "OTP not found." };
        }

        var validation = await ValidateOtpAsync(otp, request.Otp, cancellationToken);
        if (!validation.Success)
        {
            return new AuthResponse { Success = false, Message = validation.Message };
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return new AuthResponse { Success = false, Message = "User not found." };
        }

        var resetToken = jwtService.GenerateResetToken(user.Id, user.Email);
        await WriteAuditAsync(user.Id, "AUTH.RESET_OTP_VERIFIED", "users", user.Id, ipAddress, userAgent, cancellationToken);

        return new AuthResponse
        {
            Success = true,
            Message = "OTP verified.",
            UserId = user.Id,
            AccessToken = resetToken
        };
    }

    public async Task<OperationResponse> ResetPasswordAsync(ResetPasswordRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var (isValid, userId, _) = jwtService.ValidateResetToken(request.ResetToken);
        if (!isValid)
        {
            return new OperationResponse { Success = false, Message = "Invalid or expired reset token." };
        }

        var user = await userRepository.GetByIdWithAuthGraphAsync(userId, cancellationToken);
        if (user is null)
        {
            return new OperationResponse { Success = false, Message = "User not found." };
        }

        user.PasswordHash = passwordService.Hash(request.NewPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutUntil = null;
        user.Status = UserStatus.Active;
        await userRepository.UpdateAsync(user, cancellationToken);
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        await WriteAuditAsync(user.Id, "AUTH.PASSWORD_RESET", "users", user.Id, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Password reset successful." };
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(request.RefreshToken);
        var existing = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (existing is null || !existing.IsActive)
        {
            return new AuthResponse { Success = false, Message = "Invalid refresh token." };
        }

        await refreshTokenRepository.RevokeAsync(existing, cancellationToken);

        var user = existing.User;
        if (user.Status != UserStatus.Active || !user.EmailVerified)
        {
            return new AuthResponse { Success = false, Message = "User is not allowed." };
        }

        var roles = user.UserRoles.Select(x => x.Role.Name).Distinct().ToArray();
        var permissions = user.UserRoles.SelectMany(x => x.Role.RolePermissions.Select(rp => rp.Permission.Name)).Distinct().ToArray();
        var (accessToken, expiresAt) = jwtService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        await refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHasher.Hash(newRefreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_security.RefreshTokenDays),
            Device = request.Device,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);

        await WriteAuditAsync(user.Id, "AUTH.REFRESH_TOKEN", "refresh_tokens", existing.Id, ipAddress, userAgent, cancellationToken);

        return new AuthResponse
        {
            Success = true,
            Message = "Token refreshed successfully.",
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            UserInfo = new UserInfoDto(user.Id, user.FullName, user.Email, user.Phone, roles)
        };
    }

    public async Task<OperationResponse> LogoutAsync(LogoutRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var token = await refreshTokenRepository.GetByHashAsync(tokenHasher.Hash(request.RefreshToken), cancellationToken);
        if (token is null)
        {
            return new OperationResponse { Success = true, Message = "Logged out successfully." };
        }

        await refreshTokenRepository.RevokeAsync(token, cancellationToken);
        await WriteAuditAsync(token.UserId, "AUTH.LOGOUT", "refresh_tokens", token.Id, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Logged out successfully." };
    }

    public async Task<IReadOnlyCollection<SessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await refreshTokenRepository.GetActiveSessionsAsync(userId, cancellationToken);
        return sessions
            .Select(x => new SessionDto(x.Id, x.CreatedAt, x.ExpiresAt, x.Device, x.IpAddress, x.UserAgent))
            .ToArray();
    }

    public async Task<OperationResponse> RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await refreshTokenRepository.GetByIdForUserAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            return new OperationResponse { Success = false, Message = "Session not found." };
        }

        await refreshTokenRepository.RevokeAsync(session, cancellationToken);
        return new OperationResponse { Success = true, Message = "Session revoked." };
    }

    public async Task<OperationResponse> RevokeAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);
        return new OperationResponse { Success = true, Message = "All sessions revoked." };
    }

    private async Task CreateAndSendOtpAsync(User user, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        var otpRaw = otpService.GenerateOtp();
        var otp = new EmailOtp
        {
            UserId = user.Id,
            Email = user.Email,
            OtpHash = otpService.HashOtp(otpRaw),
            Purpose = purpose,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_security.OtpMinutes),
            Attempts = 0,
            IsUsed = false
        };

        await otpRepository.AddAsync(otp, cancellationToken);
        await emailService.SendOtpEmailAsync(user.Email, otpRaw, purpose.ToString(), cancellationToken);
    }

    private async Task<OperationResponse> ValidateOtpAsync(EmailOtp otp, string rawOtp, CancellationToken cancellationToken)
    {
        if (otp.IsUsed)
        {
            return new OperationResponse { Success = false, Message = "OTP already used." };
        }

        if (otp.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return new OperationResponse { Success = false, Message = "OTP expired." };
        }

        if (otp.Attempts >= _security.OtpMaxAttempts)
        {
            return new OperationResponse { Success = false, Message = "OTP max attempts exceeded." };
        }

        if (!otpService.VerifyOtp(rawOtp, otp.OtpHash))
        {
            otp.Attempts += 1;
            await otpRepository.UpdateAsync(otp, cancellationToken);
            return new OperationResponse { Success = false, Message = "Invalid OTP." };
        }

        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp, cancellationToken);
        return new OperationResponse { Success = true, Message = "OTP verified." };
    }

    private async Task WriteAuditAsync(Guid? actorUserId, string action, string entityType, Guid? entityId, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
