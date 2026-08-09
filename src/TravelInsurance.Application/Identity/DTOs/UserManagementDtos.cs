using System.ComponentModel.DataAnnotations;

namespace TravelInsurance.Application.Identity.DTOs;

public record RoleDto(int Id, string Name);

public record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string Status,
    bool EmailVerified,
    Guid? CompanyId,
    string? CompanyName,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<RoleDto> Roles);

public class AssignRoleRequest
{
    [Required]
    public int RoleId { get; set; }
}

public class AssignCompanyRequest
{
    public Guid? CompanyId { get; set; }
}
