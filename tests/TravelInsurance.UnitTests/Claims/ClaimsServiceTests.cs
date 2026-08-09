using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Claims.Abstractions;
using TravelInsurance.Application.Claims.DTOs;
using TravelInsurance.Application.Claims.Services;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.UnitTests.Claims;

public class ClaimsServiceTests
{
    [Fact]
    public async Task ClaimWorkflow_ApprovesPaysAndClosesAClaimForInsuranceCompany()
    {
        var companyId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var passenger = new PolicyPassenger { DateOfBirth = new DateOnly(1990, 1, 1) };
        var policy = new Policy
        {
            Plan = new InsurancePlan { CompanyId = companyId },
            Currency = "USD", EffectiveFrom = new DateOnly(2027, 1, 1), EffectiveTo = new DateOnly(2027, 1, 31),
            Passengers = { passenger }
        };
        var repository = new FakeClaimsRepository { Actor = new WorkflowActor(actorId, companyId, CompanyType.Insurance, false), Policy = policy };
        var service = new ClaimsService(repository, new FakeCountryRepository(), new FakeAuditLogRepository(), new FakeNumberSequenceService());

        var (registered, registration) = await service.RegisterAsync(new RegisterClaimRequest
        {
            PolicyId = policy.Id, PolicyPassengerId = passenger.Id, ClaimType = ClaimType.Medical,
            IncidentDate = new DateOnly(2027, 1, 10), IncidentDescription = "Emergency treatment", RequestedAmount = 100, Currency = "USD"
        }, actorId);
        Assert.True(registration.Success);
        Assert.Equal(ClaimStatus.Registered, registered!.Status);

        var (_, review) = await service.UpdateStatusAsync(registered.Id, new UpdateClaimStatusRequest { Status = ClaimStatus.UnderReview }, actorId);
        Assert.True(review.Success);
        var (_, approval) = await service.ApproveAsync(registered.Id, new ApproveClaimRequest { ApprovedAmount = 100 }, actorId);
        Assert.True(approval.Success);
        var (partPaid, firstPayment) = await service.RecordPaymentAsync(registered.Id, new RecordClaimPaymentRequest { Amount = 40, Currency = "USD" }, actorId);
        Assert.True(firstPayment.Success);
        Assert.Equal(ClaimStatus.Approved, partPaid!.Status);
        var (paid, secondPayment) = await service.RecordPaymentAsync(registered.Id, new RecordClaimPaymentRequest { Amount = 60, Currency = "USD" }, actorId);
        Assert.True(secondPayment.Success);
        Assert.Equal(ClaimStatus.Paid, paid!.Status);
        var (closed, close) = await service.CloseAsync(registered.Id, new CloseClaimRequest(), actorId);
        Assert.True(close.Success);
        Assert.Equal(ClaimStatus.Closed, closed!.Status);
        Assert.Equal(4, closed.StatusHistory.Count);
    }

    private sealed class FakeClaimsRepository : IClaimsRepository
    {
        public WorkflowActor? Actor { get; set; }
        public Policy? Policy { get; set; }
        public List<Claim> Claims { get; } = new();
        public Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Actor?.UserId == userId ? Actor : null);
        public Task<Policy?> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default) => Task.FromResult(Policy?.Id == policyId ? Policy : null);
        public Task<Claim?> GetClaimAsync(Guid claimId, CancellationToken cancellationToken = default) => Task.FromResult(Claims.FirstOrDefault(c => c.Id == claimId));
        public Task<List<Claim>> GetClaimsAsync(Guid? insuranceCompanyId, CancellationToken cancellationToken = default) => Task.FromResult(Claims.ToList());
        public void AddClaim(Claim claim) { claim.Policy = Policy; Claims.Add(claim); }
        public Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(WorkflowSaveResult.Saved);
    }

    private sealed class FakeCountryRepository : ICountryRepository
    {
        public Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Country>());
        public Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) => Task.FromResult(new HashSet<Guid>());
    }

    private sealed class FakeAuditLogRepository : IAuditLogRepository
    {
        public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeNumberSequenceService : INumberSequenceService
    {
        public Task<string> NextAsync(Guid companyId, NumberSequenceType type, CancellationToken cancellationToken = default) =>
            Task.FromResult("CLM-2026-000001");
    }
}

