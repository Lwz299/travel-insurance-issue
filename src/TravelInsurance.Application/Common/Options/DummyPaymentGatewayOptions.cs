namespace TravelInsurance.Application.Common.Options;

public class DummyPaymentGatewayOptions
{
    public const string SectionName = "DummyPaymentGateway";
    public string Scenario { get; set; } = "AlwaysSuccess";
}
