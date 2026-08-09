using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TravelInsurance.Application.Issuance.Abstractions;

namespace TravelInsurance.Infrastructure.Persistence;

public sealed class QuoteExpirationHostedService(IServiceScopeFactory scopeFactory, ILogger<QuoteExpirationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IInsuranceWorkflowRepository>();
                var expired = await repository.ExpireOutstandingQuotesAsync(DateTimeOffset.UtcNow, stoppingToken);
                if (expired > 0) logger.LogInformation("Expired {QuoteCount} outstanding quotes.", expired);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Unable to expire outstanding quotes."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
