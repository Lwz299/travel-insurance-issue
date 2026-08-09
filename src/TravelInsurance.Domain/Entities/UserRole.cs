namespace TravelInsurance.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
