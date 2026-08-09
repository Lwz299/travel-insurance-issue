namespace TravelInsurance.Domain.Entities;

/// <summary>System catalog coverage definition (no limits — limits live on mappings and plan assignments).</summary>
public class Coverage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public int RiskTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public CoverageGroup? Group { get; set; }
    public RiskType? RiskType { get; set; }
    public ICollection<CoverageBenefit> CoverageBenefits { get; set; } = new List<CoverageBenefit>();
    public ICollection<PlanCoverageAssignment> PlanAssignments { get; set; } = new List<PlanCoverageAssignment>();
}
