using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Application.Payments.Abstractions;

public interface IPaymentGateway
{
    string ProviderCode { get; }
    Task<GatewayCreateResult> CreateAsync(GatewayCreateRequest request, CancellationToken cancellationToken = default);
    Task<GatewayStatusResult> GetStatusAsync(string providerReference, CancellationToken cancellationToken = default);
    Task<GatewayRefundResult> RefundAsync(GatewayRefundRequest request, CancellationToken cancellationToken = default);
    Task<GatewayWebhookResult> VerifyWebhookAsync(GatewayWebhookRequest request, CancellationToken cancellationToken = default);
}
