namespace TravelInsurance.Application.Payments.Abstractions;

public interface IPaymentGatewayResolver
{
    IPaymentGateway? Resolve(string providerCode);
}
