using TravelInsurance.Application.Premium.DTOs;

namespace TravelInsurance.Application.Premium.Abstractions;

public interface IPremiumCalculationService
{
    Task<PremiumCalculationResponse> CalculateAsync(CalculatePremiumRequest request, CancellationToken cancellationToken = default);
}
