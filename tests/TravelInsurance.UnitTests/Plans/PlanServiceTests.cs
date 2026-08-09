using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;
using TravelInsurance.Application.Plans.Services;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.UnitTests.Plans;

public class PlanServiceTests
{
    private static readonly Guid InsuranceCompanyId = Guid.NewGuid();
    private static readonly Guid AgencyCompanyId = Guid.NewGuid();
    private static readonly Guid KnownCountryId = Guid.NewGuid();

    private static PlanService CreateSut(
        out FakePlanRepository plans,
        out FakeCompanyRepository companies,
        out FakeCountryRepository countries)
    {
        plans = new FakePlanRepository();
        companies = new FakeCompanyRepository
        {
            Companies =
            {
                [InsuranceCompanyId] = new Company { Id = InsuranceCompanyId, Name = "Acme Insurance", Type = CompanyType.Insurance },
                [AgencyCompanyId] = new Company { Id = AgencyCompanyId, Name = "Travel Agency", Type = CompanyType.Agency }
            }
        };
        countries = new FakeCountryRepository { ExistingIds = { KnownCountryId } };
        return new PlanService(plans, companies, countries, new FakeCoverageZoneRepository(), new FakeAuditLogRepository());
    }

    private static CreatePlanRequest ValidRequest() => new()
    {
        CompanyId = InsuranceCompanyId,
        Name = "Worldwide Gold",
        CoverageType = CoverageType.SingleTrip,
        MinAge = 1,
        MaxAge = 70
    };

    [Fact]
    public async Task CreateAsync_Fails_WhenCompanyNotFound()
    {
        var sut = CreateSut(out _, out _, out _);
        var request = ValidRequest();
        request.CompanyId = Guid.NewGuid();

        var (plan, result) = await sut.CreateAsync(request, null, null, null);

        Assert.Null(plan);
        Assert.False(result.Success);
        Assert.Equal("Company not found.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Fails_WhenCompanyIsNotInsurance()
    {
        var sut = CreateSut(out _, out _, out _);
        var request = ValidRequest();
        request.CompanyId = AgencyCompanyId;

        var (plan, result) = await sut.CreateAsync(request, null, null, null);

        Assert.Null(plan);
        Assert.False(result.Success);
        Assert.Contains("insurance company", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Fails_WhenMinAgeGreaterThanMaxAge()
    {
        var sut = CreateSut(out _, out _, out _);
        var request = ValidRequest();
        request.MinAge = 80;
        request.MaxAge = 60;

        var (_, result) = await sut.CreateAsync(request, null, null, null);

        Assert.False(result.Success);
        Assert.Contains("MinAge", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Fails_WhenPricingRuleDurationInverted()
    {
        var sut = CreateSut(out _, out _, out _);
        var request = ValidRequest();
        request.PricingRules.Add(new PricingRuleInput { MinAge = 1, MaxAge = 70, DurationFrom = 30, DurationTo = 10, Price = 50 });

        var (_, result) = await sut.CreateAsync(request, null, null, null);

        Assert.False(result.Success);
        Assert.Contains("DurationFrom", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Fails_WhenDuplicateCountries()
    {
        var sut = CreateSut(out _, out _, out _);
        var request = ValidRequest();
        request.Countries.Add(new PlanCountryInput { CountryId = KnownCountryId });
        request.Countries.Add(new PlanCountryInput { CountryId = KnownCountryId });

        var (_, result) = await sut.CreateAsync(request, null, null, null);

        Assert.False(result.Success);
        Assert.Contains("Duplicate", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Fails_WhenCountryDoesNotExist()
    {
        var sut = CreateSut(out _, out _, out _);
        var request = ValidRequest();
        request.Countries.Add(new PlanCountryInput { CountryId = Guid.NewGuid() });

        var (_, result) = await sut.CreateAsync(request, null, null, null);

        Assert.False(result.Success);
        Assert.Contains("countries do not exist", result.Message);
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WithValidInsuranceCompany()
    {
        var sut = CreateSut(out var plans, out _, out _);
        var request = ValidRequest();
        request.Countries.Add(new PlanCountryInput { CountryId = KnownCountryId });
        request.PricingRules.Add(new PricingRuleInput { MinAge = 1, MaxAge = 70, DurationFrom = 1, DurationTo = 30, Price = 25 });

        var (plan, result) = await sut.CreateAsync(request, null, null, null);

        Assert.True(result.Success);
        Assert.NotNull(plan);
        Assert.Single(plans.Stored);
        Assert.True(plans.Stored[0].IsActive);
        Assert.Equal("Worldwide Gold", plan!.Name);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes_AndDeactivates()
    {
        var sut = CreateSut(out var plans, out _, out _);
        var existing = new InsurancePlan { CompanyId = InsuranceCompanyId, Name = "Old", IsActive = true };
        plans.Stored.Add(existing);

        var result = await sut.DeleteAsync(existing.Id, null, null, null);

        Assert.True(result.Success);
        Assert.True(existing.IsDeleted);
        Assert.False(existing.IsActive);
    }

    [Fact]
    public async Task SetStatusAsync_Fails_WhenPlanMissing()
    {
        var sut = CreateSut(out _, out _, out _);

        var result = await sut.SetStatusAsync(Guid.NewGuid(), false, null, null, null);

        Assert.False(result.Success);
        Assert.Equal("Plan not found.", result.Message);
    }

    // ---------- Fakes ----------

    private sealed class FakePlanRepository : IPlanRepository
    {
        public List<InsurancePlan> Stored { get; } = new();

        public Task<List<InsurancePlan>> GetAllAsync(Guid? companyId, bool? isActive, CancellationToken cancellationToken = default)
            => Task.FromResult(Stored.ToList());

        public Task<InsurancePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Stored.FirstOrDefault(p => p.Id == id));

        public Task<InsurancePlan?> GetByIdWithGraphAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Stored.FirstOrDefault(p => p.Id == id));

        public Task AddAsync(InsurancePlan plan, CancellationToken cancellationToken = default)
        {
            Stored.Add(plan);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(InsurancePlan plan, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        public Dictionary<Guid, Company> Companies { get; } = new();

        public Task<List<Company>> GetAllAsync(CompanyType? type, CancellationToken cancellationToken = default)
            => Task.FromResult(Companies.Values.ToList());

        public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Companies.TryGetValue(id, out var c) ? c : null);

        public Task AddAsync(Company company, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Company company, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> HasUsersAsync(Guid companyId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task DeleteAsync(Company company, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCountryRepository : ICountryRepository
    {
        public HashSet<Guid> ExistingIds { get; } = new();

        public Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ExistingIds.Select(id => new Country { Id = id }).ToList());

        public Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
            => Task.FromResult(ids.Where(ExistingIds.Contains).ToHashSet());
    }

    private sealed class FakeAuditLogRepository : Application.Authentication.Abstractions.IAuditLogRepository
    {
        public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCoverageZoneRepository : ICoverageZoneRepository
    {
        public Task<List<CoverageZone>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<CoverageZone>());
        public Task<CoverageZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<CoverageZone?>(null);
        public Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) => Task.FromResult(new HashSet<Guid>());
        public Task AddAsync(CoverageZone zone, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(CoverageZone zone, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
