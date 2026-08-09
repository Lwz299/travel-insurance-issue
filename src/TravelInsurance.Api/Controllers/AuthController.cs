using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("auth")]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResponse>> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.VerifyEmailAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResponse>> ResendOtp([FromBody] ResendOtpRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.ResendOtpAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.ForgotPasswordAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("verify-reset-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> VerifyResetOtp([FromBody] VerifyResetOtpRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.VerifyResetOtpAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.ResetPasswordAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RefreshAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResponse>> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LogoutAsync(request, ClientIp(), UserAgent(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<SessionDto>>> GetSessions(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var sessions = await authService.GetSessionsAsync(userId.Value, cancellationToken);
        return Ok(sessions);
    }

    [HttpPost("sessions/revoke")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AUTH.MANAGE_SESSIONS")]
    public async Task<ActionResult<OperationResponse>> RevokeSession([FromBody] RevokeSessionRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await authService.RevokeSessionAsync(userId.Value, request.SessionId, cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<ActionResult<OperationResponse>> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await authService.RevokeAllSessionsAsync(userId.Value, cancellationToken);
        return Ok(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
