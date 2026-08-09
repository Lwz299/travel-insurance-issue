using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Application.Premium.Services;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.UnitTests.Premium;

public class PremiumCalculationServiceTests
{
    [Fact]
    public async Task CalculateAsync_ReturnsTotalAndCoverageSnapshot_ForMatchingPassengers()
    {
        var planId = Guid.NewGuid();
        var repository = new FakeWorkflowRepository
        {
            Plan = new InsurancePlan
            {
                Id = planId,
                MinAge = 1,
                MaxAge = 75,
                MaxTripDurationDays = 30,
                PricingRules =
                {
                    new PricingRule { MinAge = 1, MaxAge = 75, DurationFrom = 1, DurationTo = 30, Price = 25, Currency = "USD" }
                },
                CoverageAssignments =
                {
                    new PlanCoverageAssignment
                    {
                        CoverageId = Guid.NewGuid(), CustomLimit = 100_000, Currency = "USD",
                        Coverage = new Coverage { Code = "MEDICAL", Name = "Medical" }
                    }
                }
            }
        };
        var service = new PremiumCalculationService(repository);

        var result = await service.CalculateAsync(new CalculatePremiumRequest
        {
            PlanId = planId,
            DestinationCountryId = Guid.NewGuid(),
            TravelStart = new DateOnly(2026, 8, 1),
            TravelEnd = new DateOnly(2026, 8, 7),
            Passengers =
            {
                new PassengerInput { FullName = "One", DateOfBirth = new DateOnly(1990, 1, 1) },
                new PassengerInput { FullName = "Two", DateOfBirth = new DateOnly(2000, 1, 1) }
            }
        });

        Assert.True(result.Success);
        Assert.Equal(50, result.GrossPremium);
        Assert.Equal(7, result.DurationDays);
        Assert.Single(result.CoverageSnapshots);
    }

    [Fact]
    public async Task CalculateAsync_Fails_WhenDestinationIsNotCovered()
    {
        var planId = Guid.NewGuid();
        var repository = new FakeWorkflowRepository
        {
            Plan = new InsurancePlan
            {
                Id = planId,
                Countries = { new PlanCountry { CountryId = Guid.NewGuid() } },
                PricingRules = { new PricingRule { MinAge = 1, MaxAge = 75, DurationFrom = 1, DurationTo = 30, Price = 25 } }
            }
        };
        var service = new PremiumCalculationService(repository);

        var result = await service.CalculateAsync(new CalculatePremiumRequest
        {
            PlanId = planId,
            DestinationCountryId = Guid.NewGuid(),
            TravelStart = new DateOnly(2026, 8, 1), TravelEnd = new DateOnly(2026, 8, 2),
            Passengers = { new PassengerInput { FullName = "One", DateOfBirth = new DateOnly(1990, 1, 1) } }
        });

        Assert.False(result.Success);
        Assert.Contains("destination", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CalculateAsync_UsesCoverageZoneAndHonorsWorldwideExclusions()
    {
        var planId = Guid.NewGuid();
        var excludedCountryId = Guid.NewGuid();
        var includedCountryId = Guid.NewGuid();
        var repository = new FakeWorkflowRepository
        {
            Plan = new InsurancePlan
            {
                Id = planId,
                PricingRules = { new PricingRule { MinAge = 1, MaxAge = 75, DurationFrom = 1, DurationTo = 30, Price = 25, Currency = "USD" } },
                CoverageZones =
                {
                    new PlanCoverageZone
                    {
                        CoverageZone = new CoverageZone
                        {
                            IsWorldwide = true,
                            IsActive = true,
                            Countries = { new CoverageZoneCountry { CountryId = excludedCountryId, IsExcluded = true } }
                        }
                    }
                }
            }
        };
        var service = new PremiumCalculationService(repository);
        var request = new CalculatePremiumRequest
        {
            PlanId = planId,
            TravelStart = new DateOnly(2030, 8, 1),
            TravelEnd = new DateOnly(2030, 8, 2),
            Passengers = { new PassengerInput { FullName = "One", DateOfBirth = new DateOnly(1990, 1, 1) } }
        };

        request.DestinationCountryId = includedCountryId;
        Assert.True((await service.CalculateAsync(request)).Success);

        request.DestinationCountryId = excludedCountryId;
        Assert.False((await service.CalculateAsync(request)).Success);
    }

    private sealed class FakeWorkflowRepository : IInsuranceWorkflowRepository
    {
        public InsurancePlan? Plan { get; set; }
        public Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<WorkflowActor?>(null);
        public Task<InsurancePlan?> GetPlanForIssuanceAsync(Guid planId, CancellationToken cancellationToken = default) => Task.FromResult(Plan?.Id == planId ? Plan : null);
        public Task<Quote?> GetQuoteForIssuanceAsync(Guid quoteId, CancellationToken cancellationToken = default) => Task.FromResult<Quote?>(null);
        public Task<bool> HasPaidQuotePaymentAsync(Guid quoteId, decimal amount, string currency, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> HasPolicyForQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Policy?> GetPolicyForWorkflowAsync(Guid policyId, CancellationToken cancellationToken = default) => Task.FromResult<Policy?>(null);
        public Task<List<Policy>> GetPoliciesForWorkflowAsync(Guid? companyId, CancellationToken cancellationToken = default) => Task.FromResult(new List<Policy>());
        public Task<Payment?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default) => Task.FromResult<Payment?>(null);
        public Task<Payment?> GetPaymentByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<Payment?>(null);
        public Task<Policy?> GetPolicyByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<Policy?>(null);
        public Task<PolicyIssuanceRequest?> GetIssuanceRequestAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default) => Task.FromResult<PolicyIssuanceRequest?>(null);
        public Task<PolicyIssuanceRequest?> GetIssuanceRequestReadOnlyAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default) => Task.FromResult<PolicyIssuanceRequest?>(null);
        public Task<PolicyIssuanceRequest?> GetIssuanceRequestByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<PolicyIssuanceRequest?>(null);
        public Task<PolicyIssuanceRequest?> GetIssuanceRequestByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default) => Task.FromResult<PolicyIssuanceRequest?>(null);
        public Task<int> ExpireOutstandingQuotesAsync(DateTimeOffset now, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public void AddQuote(Quote quote) { }
        public void AddPayment(Payment payment) { }
        public void AddPolicy(Policy policy) { }
        public void AddIssuanceRequest(PolicyIssuanceRequest issuanceRequest) { }
        public Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(WorkflowSaveResult.Saved);
    }
}
