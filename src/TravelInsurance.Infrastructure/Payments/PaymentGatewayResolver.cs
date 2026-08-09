using TravelInsurance.Application.Payments.Abstractions;

namespace TravelInsurance.Infrastructure.Payments;

public class PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways) : IPaymentGatewayResolver
{
    public IPaymentGateway? Resolve(string providerCode) =>
        gateways.FirstOrDefault(g => string.Equals(g.ProviderCode, providerCode, StringComparison.OrdinalIgnoreCase));
}
