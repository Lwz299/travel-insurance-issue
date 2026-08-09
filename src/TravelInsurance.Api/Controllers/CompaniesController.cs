using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Organization.DTOs;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("companies")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "COMPANY.MANAGE")]
public class CompaniesController(ICompanyService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompanyResponse>>> GetAll([FromQuery] CompanyType? type, CancellationToken cancellationToken)
    {
        var companies = await companyService.GetAllAsync(type, cancellationToken);
        return Ok(companies);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var company = await companyService.GetByIdAsync(id, cancellationToken);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var created = await companyService.CreateAsync(request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var updated = await companyService.UpdateAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<OperationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await companyService.DeleteAsync(id, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
