using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Api.Controllers.Catalog;

[ApiController]
[Route("catalog/groups")]
[Authorize]
public class CatalogGroupsController(ICoverageGroupService groupService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CoverageGroupResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var groups = await groupService.GetAllAsync(cancellationToken);
        return Ok(groups);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CoverageGroupResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await groupService.GetByIdAsync(id, cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<CoverageGroupResponse>> Create([FromBody] CreateCoverageGroupRequest request, CancellationToken cancellationToken)
    {
        var (group, result) = await groupService.CreateAsync(request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || group is null)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<CoverageGroupResponse>> Update(Guid id, [FromBody] UpdateCoverageGroupRequest request, CancellationToken cancellationToken)
    {
        var (group, result) = await groupService.UpdateAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || group is null)
        {
            return result.Message == "Coverage group not found." ? NotFound(result) : BadRequest(result);
        }

        return Ok(group);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await groupService.DeleteAsync(id, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
