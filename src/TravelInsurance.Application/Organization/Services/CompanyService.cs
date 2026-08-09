using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Organization.DTOs;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Organization.Services;

public class CompanyService(
    ICompanyRepository companyRepository,
    IAuditLogRepository auditLogRepository) : ICompanyService
{
    public async Task<IReadOnlyCollection<CompanyResponse>> GetAllAsync(CompanyType? type, CancellationToken cancellationToken = default)
    {
        var companies = await companyRepository.GetAllAsync(type, cancellationToken);
        return companies.Select(Map).ToArray();
    }

    public async Task<CompanyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        return company is null ? null : Map(company);
    }

    public async Task<CompanyResponse> CreateAsync(CreateCompanyRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var company = new Company
        {
            Type = request.Type,
            Name = request.Name.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            Phone = request.Phone?.Trim(),
            Website = request.Website?.Trim(),
            Address = request.Address?.Trim(),
            TaxNumber = request.TaxNumber?.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            IsActive = true
        };

        await companyRepository.AddAsync(company, cancellationToken);
        await WriteAuditAsync(actorUserId, "COMPANY.CREATED", company.Id, ipAddress, userAgent, cancellationToken);
        return Map(company);
    }

    public async Task<CompanyResponse?> UpdateAsync(Guid id, UpdateCompanyRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return null;
        }

        company.Name = request.Name.Trim();
        company.LogoUrl = request.LogoUrl?.Trim();
        company.Email = request.Email?.Trim().ToLowerInvariant();
        company.Phone = request.Phone?.Trim();
        company.Website = request.Website?.Trim();
        company.Address = request.Address?.Trim();
        company.TaxNumber = request.TaxNumber?.Trim();
        company.RegistrationNumber = request.RegistrationNumber?.Trim();
        company.IsActive = request.IsActive;

        await companyRepository.UpdateAsync(company, cancellationToken);
        await WriteAuditAsync(actorUserId, "COMPANY.UPDATED", company.Id, ipAddress, userAgent, cancellationToken);
        return Map(company);
    }

    public async Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return new OperationResponse { Success = false, Message = "Company not found." };
        }

        if (await companyRepository.HasUsersAsync(id, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Cannot delete a company that has assigned users." };
        }

        await companyRepository.DeleteAsync(company, cancellationToken);
        await WriteAuditAsync(actorUserId, "COMPANY.DELETED", company.Id, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Company deleted." };
    }

    private async Task WriteAuditAsync(Guid? actorUserId, string action, Guid entityId, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "companies",
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);
    }

    private static CompanyResponse Map(Company c) => new(
        c.Id, c.Type, c.Name, c.LogoUrl, c.Email, c.Phone, c.Website, c.Address,
        c.TaxNumber, c.RegistrationNumber, c.IsActive, c.CreatedAt, c.UpdatedAt);
}
