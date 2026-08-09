using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.DTOs;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Payments;

public class DummyPaymentGateway(IDummyPaymentScenarioResolver scenarioResolver) : IPaymentGateway
{
    public string ProviderCode => "DUMMY";

    public Task<GatewayCreateResult> CreateAsync(GatewayCreateRequest request, CancellationToken cancellationToken = default)
    {
        var scenario = scenarioResolver.Resolve(request);
        var reference = $"DUMMY-{scenario}-{Guid.NewGuid():N}";
        return Task.FromResult(scenario switch
        {
            "AlwaysSuccess" => new GatewayCreateResult(PaymentAttemptStatus.Succeeded, reference, null, null, null),
            "AlwaysFail" => new GatewayCreateResult(PaymentAttemptStatus.Failed, reference, null, "DUMMY_FAILED", "Dummy gateway forced failure."),
            "RequiresAction" => new GatewayCreateResult(PaymentAttemptStatus.RequiresAction, reference, $"https://dummy.local/checkout/{reference}", null, null),
            "Timeout" => new GatewayCreateResult(PaymentAttemptStatus.Pending, reference, null, null, null, "timeout-simulated"),
            "Random" => Random.Shared.Next(0, 2) == 0
                ? new GatewayCreateResult(PaymentAttemptStatus.Succeeded, reference, null, null, null)
                : new GatewayCreateResult(PaymentAttemptStatus.Failed, reference, null, "DUMMY_RANDOM_FAILURE", "Dummy gateway random failure."),
            _ => new GatewayCreateResult(PaymentAttemptStatus.Pending, reference, null, null, null)
        });
    }

    public Task<GatewayStatusResult> GetStatusAsync(string providerReference, CancellationToken cancellationToken = default)
    {
        if (providerReference.Contains("PendingThenSuccess", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(new GatewayStatusResult(PaymentAttemptStatus.Succeeded, null, null));
        if (providerReference.Contains("PendingThenFail", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(new GatewayStatusResult(PaymentAttemptStatus.Failed, "DUMMY_FAILED", "Dummy gateway forced failure."));
        if (providerReference.Contains("AlwaysSuccess", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(new GatewayStatusResult(PaymentAttemptStatus.Succeeded, null, null));
        if (providerReference.Contains("AlwaysFail", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(new GatewayStatusResult(PaymentAttemptStatus.Failed, "DUMMY_FAILED", "Dummy gateway forced failure."));
        return Task.FromResult(new GatewayStatusResult(PaymentAttemptStatus.Pending, null, null));
    }

    public Task<GatewayRefundResult> RefundAsync(GatewayRefundRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayRefundResult(true, $"DUMMY-REFUND-{Guid.NewGuid():N}", null, null));

    public async Task<GatewayWebhookResult> VerifyWebhookAsync(GatewayWebhookRequest request, CancellationToken cancellationToken = default)
    {
        var providerReference = request.Payload.Trim();
        if (string.IsNullOrWhiteSpace(providerReference)) return new GatewayWebhookResult(false, null, null, "INVALID_PAYLOAD", "Provider reference is required.");
        var status = await GetStatusAsync(providerReference, cancellationToken);
        return new GatewayWebhookResult(true, providerReference, status.Status, status.FailureCode, status.FailureMessage, status.SafeMetadata);
    }
}
