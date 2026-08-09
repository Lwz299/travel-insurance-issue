using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Common.Options;

namespace TravelInsurance.Infrastructure.Email;

public class ConsoleEmailService(ILogger<ConsoleEmailService> logger, IOptions<BrevoOptions> brevoOptions) : IEmailService
{
    private readonly BrevoOptions _brevoOptions = brevoOptions.Value;

    public Task SendOtpEmailAsync(string email, string otp, string purpose, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("OTP for {Email} ({Purpose}): {Otp}. Sender: {SenderEmail}", email, purpose, otp, _brevoOptions.SenderEmail);
        return Task.CompletedTask;
    }
}
