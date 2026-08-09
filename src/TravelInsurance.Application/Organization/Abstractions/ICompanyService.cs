using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Organization.DTOs;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Organization.Abstractions;

public interface ICompanyService
{
    Task<IReadOnlyCollection<CompanyResponse>> GetAllAsync(CompanyType? type, CancellationToken cancellationToken = default);
    Task<CompanyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CompanyResponse> CreateAsync(CreateCompanyRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<CompanyResponse?> UpdateAsync(Guid id, UpdateCompanyRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
