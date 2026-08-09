namespace TravelInsurance.Domain.Entities;

public class QuoteBenefitSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuoteCoverageSnapshotId { get; set; }
    public Guid BenefitId { get; set; }
    public string BenefitCode { get; set; } = string.Empty;
    public string BenefitName { get; set; } = string.Empty;
    public int LimitTypeId { get; set; }
    public decimal? Limit { get; set; }
    public string? Currency { get; set; }
    public decimal? Deductible { get; set; }
    public int? WaitingPeriodDays { get; set; }
    public QuoteCoverageSnapshot? QuoteCoverageSnapshot { get; set; }
}
