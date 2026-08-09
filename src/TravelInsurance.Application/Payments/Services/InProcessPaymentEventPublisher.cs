using Microsoft.Extensions.DependencyInjection;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Application.Payments.Services;

public class InProcessPaymentEventPublisher(IServiceScopeFactory scopeFactory) : IPaymentEventPublisher
{
    public async Task PublishSucceededAsync(PaymentSucceeded payment, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<IPaymentSucceededHandler>();
        foreach (var handler in handlers) await handler.HandleAsync(payment, cancellationToken);
    }
}
