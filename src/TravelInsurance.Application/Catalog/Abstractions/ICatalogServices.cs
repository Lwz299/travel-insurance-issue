using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Application.Catalog.Abstractions;

public interface ICatalogLookupService
{
    Task<IReadOnlyCollection<LookupItemResponse>> GetBenefitTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LookupItemResponse>> GetLimitTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LookupItemResponse>> GetRiskTypesAsync(CancellationToken cancellationToken = default);
}

public interface ICoverageGroupService
{
    Task<IReadOnlyCollection<CoverageGroupResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CoverageGroupResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(CoverageGroupResponse? Group, OperationResponse Result)> CreateAsync(CreateCoverageGroupRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<(CoverageGroupResponse? Group, OperationResponse Result)> UpdateAsync(Guid id, UpdateCoverageGroupRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
}

public interface ICoverageCatalogService
{
    Task<IReadOnlyCollection<CoverageResponse>> GetAllAsync(Guid? groupId, CancellationToken cancellationToken = default);
    Task<CoverageDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(CoverageResponse? Coverage, OperationResponse Result)> CreateAsync(CreateCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<(CoverageResponse? Coverage, OperationResponse Result)> UpdateAsync(Guid id, UpdateCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> SetStatusAsync(Guid id, bool isActive, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
}

public interface IBenefitCatalogService
{
    Task<IReadOnlyCollection<BenefitResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BenefitResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(BenefitResponse? Benefit, OperationResponse Result)> CreateAsync(CreateBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<(BenefitResponse? Benefit, OperationResponse Result)> UpdateAsync(Guid id, UpdateBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> SetStatusAsync(Guid id, bool isActive, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
}

public interface ICoverageBenefitMappingService
{
    Task<IReadOnlyCollection<CoverageBenefitResponse>> GetByCoverageAsync(Guid coverageId, CancellationToken cancellationToken = default);
    Task<OperationResponse> AddAsync(Guid coverageId, AddCoverageBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> UpdateAsync(Guid coverageId, Guid benefitId, UpdateCoverageBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> RemoveAsync(Guid coverageId, Guid benefitId, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
}
