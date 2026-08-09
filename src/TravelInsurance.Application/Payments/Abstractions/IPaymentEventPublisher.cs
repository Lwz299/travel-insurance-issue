using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Application.Payments.Abstractions;

public interface IPaymentEventPublisher
{
    Task PublishSucceededAsync(PaymentSucceeded payment, CancellationToken cancellationToken = default);
}

public interface IPaymentSucceededHandler
{
    Task HandleAsync(PaymentSucceeded payment, CancellationToken cancellationToken = default);
}
