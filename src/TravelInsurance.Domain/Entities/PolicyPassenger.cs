namespace TravelInsurance.Domain.Entities;

public class PolicyPassenger
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PolicyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? PassportNumber { get; set; }
    public string? Nationality { get; set; }
    public Policy? Policy { get; set; }
}
