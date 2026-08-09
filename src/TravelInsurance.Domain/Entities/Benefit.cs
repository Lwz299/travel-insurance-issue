namespace TravelInsurance.Domain.Entities;

/// <summary>System catalog benefit definition (no limits).</summary>
public class Benefit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int BenefitTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public BenefitType? BenefitType { get; set; }
    public ICollection<CoverageBenefit> CoverageBenefits { get; set; } = new List<CoverageBenefit>();
}
