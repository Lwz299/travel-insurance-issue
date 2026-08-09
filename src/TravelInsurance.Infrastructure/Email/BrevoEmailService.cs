using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Common.Options;

namespace TravelInsurance.Infrastructure.Email;

public class BrevoEmailService(
    HttpClient httpClient,
    IOptions<BrevoOptions> brevoOptions,
    ILogger<BrevoEmailService> logger) : IEmailService
{
    private readonly BrevoOptions _options = brevoOptions.Value;

    public async Task SendOtpEmailAsync(string email, string otp, string purpose, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("Brevo ApiKey is not configured. OTP for {Email} ({Purpose}): {Otp}", email, purpose, otp);
            return;
        }

        var subject = purpose switch
        {
            "Register" => "Verify your email - Travel Insurance",
            "ResetPassword" => "Reset your password - Travel Insurance",
            _ => "Your verification code - Travel Insurance"
        };

        var payload = new
        {
            sender = new { name = _options.SenderName, email = _options.SenderEmail },
            to = new[] { new { email } },
            subject,
            htmlContent = BuildHtml(otp, purpose)
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Brevo email send failed for {Email}. Status: {Status}. Body: {Body}", email, response.StatusCode, body);
            throw new InvalidOperationException($"Failed to send email via Brevo (status {(int)response.StatusCode}).");
        }

        logger.LogInformation("OTP email sent to {Email} ({Purpose}) via Brevo.", email, purpose);
    }

    private static string BuildHtml(string otp, string purpose)
    {
        var title = purpose == "ResetPassword" ? "Password Reset" : "Email Verification";
        return $$"""
        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #eee;border-radius:12px">
          <h2 style="color:#1f2937;margin:0 0 8px">Travel Insurance</h2>
          <p style="color:#374151;margin:0 0 16px">{{title}}</p>
          <p style="color:#6b7280;margin:0 0 12px">Use the following one-time code. It expires in a few minutes:</p>
          <div style="font-size:32px;font-weight:700;letter-spacing:8px;color:#111827;background:#f3f4f6;padding:16px;border-radius:8px;text-align:center">{{otp}}</div>
          <p style="color:#9ca3af;font-size:12px;margin:16px 0 0">If you did not request this, you can safely ignore this email.</p>
        </div>
        """;
    }
}
