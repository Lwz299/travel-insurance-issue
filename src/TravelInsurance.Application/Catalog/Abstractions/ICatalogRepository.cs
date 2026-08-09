using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Catalog.Abstractions;

public interface ICatalogRepository
{
    // Lookups
    Task<List<BenefitType>> GetBenefitTypesAsync(CancellationToken cancellationToken = default);
    Task<List<LimitType>> GetLimitTypesAsync(CancellationToken cancellationToken = default);
    Task<List<RiskType>> GetRiskTypesAsync(CancellationToken cancellationToken = default);
    Task<bool> BenefitTypeExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> LimitTypeExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> RiskTypeExistsAsync(int id, CancellationToken cancellationToken = default);

    // Groups
    Task<List<CoverageGroup>> GetGroupsAsync(CancellationToken cancellationToken = default);
    Task<CoverageGroup?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> GroupCodeExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken = default);
    Task AddGroupAsync(CoverageGroup group, CancellationToken cancellationToken = default);
    Task UpdateGroupAsync(CoverageGroup group, CancellationToken cancellationToken = default);
    Task DeleteGroupAsync(CoverageGroup group, CancellationToken cancellationToken = default);
    Task<bool> GroupHasCoveragesAsync(Guid groupId, CancellationToken cancellationToken = default);

    // Coverages
    Task<List<Coverage>> GetCoveragesAsync(Guid? groupId, CancellationToken cancellationToken = default);
    Task<Coverage?> GetCoverageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Coverage?> GetCoverageWithBenefitsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CoverageCodeExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken = default);
    Task AddCoverageAsync(Coverage coverage, CancellationToken cancellationToken = default);
    Task UpdateCoverageAsync(Coverage coverage, CancellationToken cancellationToken = default);
    Task<bool> CoverageIsAssignedToPlansAsync(Guid coverageId, CancellationToken cancellationToken = default);

    // Benefits
    Task<List<Benefit>> GetBenefitsAsync(CancellationToken cancellationToken = default);
    Task<Benefit?> GetBenefitByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> BenefitCodeExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken = default);
    Task AddBenefitAsync(Benefit benefit, CancellationToken cancellationToken = default);
    Task UpdateBenefitAsync(Benefit benefit, CancellationToken cancellationToken = default);
    Task<bool> BenefitIsInUseAsync(Guid benefitId, CancellationToken cancellationToken = default);

    // Coverage-Benefit mapping
    Task<CoverageBenefit?> GetCoverageBenefitAsync(Guid coverageId, Guid benefitId, CancellationToken cancellationToken = default);
    Task AddCoverageBenefitAsync(CoverageBenefit mapping, CancellationToken cancellationToken = default);
    Task UpdateCoverageBenefitAsync(CoverageBenefit mapping, CancellationToken cancellationToken = default);
    Task RemoveCoverageBenefitAsync(CoverageBenefit mapping, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
