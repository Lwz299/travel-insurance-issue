namespace TravelInsurance.Domain.Entities;

/// <summary>Maps a benefit under a coverage with default limit values (catalog level).</summary>
public class CoverageBenefit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CoverageId { get; set; }
    public Guid BenefitId { get; set; }
    public int LimitTypeId { get; set; }
    public decimal? LimitValue { get; set; }
    public string? Currency { get; set; }
    public decimal? Deductible { get; set; }
    public int? WaitingPeriodDays { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsOptional { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Coverage? Coverage { get; set; }
    public Benefit? Benefit { get; set; }
    public LimitType? LimitType { get; set; }
}
