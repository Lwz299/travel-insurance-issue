using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.Services;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.UnitTests.Reinsurance;

public class ReinsuranceCessionServiceTests
{
    [Fact]
    public async Task CreateForIssuedPolicyAsync_CreatesSnapshotForEligibleTreaty()
    {
        var planId = Guid.NewGuid();
        var insurerId = Guid.NewGuid();
        var repository = new FakeReinsuranceRepository
        {
            Agreements =
            {
                new ReinsuranceAgreement
                {
                    CompanyId = insurerId,
                    AgreementNumber = "TR-2026-01",
                    ReinsurerCompanyId = Guid.NewGuid(),
                    Type = ReinsuranceAgreementType.Treaty,
                    ReportingMode = ReinsuranceReportingMode.PeriodicBordereau,
                    EffectiveFrom = new DateOnly(2026, 1, 1),
                    EffectiveTo = new DateOnly(2030, 12, 31),
                    CededSharePercentage = 50,
                    PlanAgreements = { new PlanReinsuranceAgreement { PlanId = planId } },
                    CoverageZones = { new ReinsuranceAgreementCoverageZone { CoverageZone = new CoverageZone { IsWorldwide = true, IsActive = true } } }
                }
            }
        };
        var policy = new Policy
        {
            PlanId = planId,
            DestinationCountryId = Guid.NewGuid(),
            EffectiveFrom = new DateOnly(2027, 1, 1),
            EffectiveTo = new DateOnly(2027, 1, 7),
            GrossPremium = 110,
            Currency = "USD",
            Passengers = { new PolicyPassenger { DateOfBirth = new DateOnly(1990, 1, 1) } },
            CoverageSnapshots = { new PolicyCoverageSnapshot { Limit = 100_000, Currency = "USD" } }
        };

        await new ReinsuranceCessionService(repository).CreateForIssuedPolicyAsync(policy, insurerId);

        var cession = Assert.Single(repository.Cessions);
        Assert.Equal(55, cession.CededPremium);
        Assert.Equal(50, cession.CededSharePercentage);
        Assert.Equal(50, cession.CompanyRetentionPercentage);
        Assert.Equal(50_000, cession.CededSumInsured);
        Assert.Equal(ReinsuranceCessionStatus.PendingBordereau, cession.Status);
        Assert.Equal("TR-2026-01", cession.AgreementNumberSnapshot);
    }

    private sealed class FakeReinsuranceRepository : IReinsuranceRepository
    {
        public List<ReinsuranceAgreement> Agreements { get; } = new();
        public List<ReinsuranceCession> Cessions { get; } = new();
        public Task<List<ReinsuranceAgreement>> GetAgreementsAsync(Guid? companyId, CancellationToken cancellationToken = default) => Task.FromResult(Agreements);
        public Task<ReinsuranceAgreement?> GetAgreementAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Agreements.FirstOrDefault(a => a.Id == id));
        public Task<List<ReinsuranceAgreement>> GetAutomaticAgreementsForPlanAsync(Guid companyId, Guid planId, CancellationToken cancellationToken = default) => Task.FromResult(Agreements.Where(a => a.CompanyId == companyId && a.PlanAgreements.Any(p => p.PlanId == planId)).ToList());
        public Task<bool> HasCessionForPolicyAsync(Guid policyId, CancellationToken cancellationToken = default) => Task.FromResult(Cessions.Any(c => c.PolicyId == policyId));
        public Task<List<ReinsuranceCession>> GetCessionsAsync(Guid? policyId, Guid? agreementId, CancellationToken cancellationToken = default) => Task.FromResult(Cessions);
        public Task<ReinsuranceBordereauBatch?> GetBordereauAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<ReinsuranceBordereauBatch?>(null);
        public Task<List<ReinsuranceCession>> GetPendingBordereauCessionsAsync(Guid agreementId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) => Task.FromResult(new List<ReinsuranceCession>());
        public Task<bool> HasBordereauForPeriodAsync(Guid agreementId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public void AddAgreement(ReinsuranceAgreement agreement) => Agreements.Add(agreement);
        public void AddCession(ReinsuranceCession cession) => Cessions.Add(cession);
        public void AddBordereau(ReinsuranceBordereauBatch batch) { }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
