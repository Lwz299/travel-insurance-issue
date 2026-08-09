using Microsoft.Extensions.Options;
using TravelInsurance.Application.Common.Options;
using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Infrastructure.Payments;

public class ConfigurationDummyPaymentScenarioResolver(IOptions<DummyPaymentGatewayOptions> options) : IDummyPaymentScenarioResolver
{
    private static readonly HashSet<string> SupportedScenarios = new(StringComparer.OrdinalIgnoreCase)
    {
        "AlwaysSuccess", "AlwaysFail", "Random", "PendingThenSuccess", "PendingThenFail", "RequiresAction", "Timeout"
    };

    public string Resolve(GatewayCreateRequest request)
    {
        var configuredScenario = options.Value.Scenario?.Trim();
        return !string.IsNullOrWhiteSpace(configuredScenario) && SupportedScenarios.Contains(configuredScenario)
            ? configuredScenario
            : "AlwaysSuccess";
    }
}
