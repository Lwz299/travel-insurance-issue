using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Infrastructure.Payments;

/// <summary>
/// Resolves the deterministic test scenario for a dummy payment attempt.
/// A real gateway never uses this abstraction; it keeps test behavior outside the payment service.
/// </summary>
public interface IDummyPaymentScenarioResolver
{
    string Resolve(GatewayCreateRequest request);
}
