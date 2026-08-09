using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.DTOs;
using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.PolicyIssuance.Abstractions;

namespace TravelInsurance.Application.PolicyIssuance.Services;

public class PaymentIssuanceCompletionHandler(IPolicyIssuanceService policyIssuanceService, IAgencyFinanceService agencyFinanceService) : IPaymentSucceededHandler
{
    public async Task HandleAsync(PaymentSucceeded payment, CancellationToken cancellationToken = default)
    {
        if (!payment.IssuanceRequestId.HasValue) return;
        await agencyFinanceService.ConfirmElectronicPaymentAsync(payment.IssuanceRequestId.Value, cancellationToken);
        await policyIssuanceService.HandlePaymentSucceededAsync(payment.IssuanceRequestId.Value, cancellationToken);
    }
}

