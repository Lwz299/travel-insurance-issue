namespace TravelInsurance.Domain.Entities;

public class QuoteCoverageSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuoteId { get; set; }
    public Guid CoverageId { get; set; }
    public string CoverageCode { get; set; } = string.Empty;
    public string CoverageName { get; set; } = string.Empty;
    public decimal? Limit { get; set; }
    public string? Currency { get; set; }
    public decimal? Deductible { get; set; }
    public Quote? Quote { get; set; }
    public ICollection<QuoteBenefitSnapshot> BenefitSnapshots { get; set; } = new List<QuoteBenefitSnapshot>();
}
