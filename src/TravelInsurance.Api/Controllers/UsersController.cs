using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Identity.Abstractions;
using TravelInsurance.Application.Identity.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "USER.MANAGE")]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userManagementService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyCollection<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await userManagementService.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManagementService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult<OperationResponse>> AssignRole(Guid id, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await userManagementService.AssignRoleAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id:guid}/roles/{roleId:int}")]
    public async Task<ActionResult<OperationResponse>> RemoveRole(Guid id, int roleId, CancellationToken cancellationToken)
    {
        var response = await userManagementService.RemoveRoleAsync(id, roleId, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{id:guid}/company")]
    public async Task<ActionResult<OperationResponse>> AssignCompany(Guid id, [FromBody] AssignCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await userManagementService.AssignCompanyAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
