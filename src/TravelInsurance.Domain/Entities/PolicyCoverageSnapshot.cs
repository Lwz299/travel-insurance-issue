namespace TravelInsurance.Domain.Entities;

public class PolicyCoverageSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PolicyId { get; set; }
    public Guid CoverageId { get; set; }
    public string CoverageCode { get; set; } = string.Empty;
    public string CoverageName { get; set; } = string.Empty;
    public decimal? Limit { get; set; }
    public string? Currency { get; set; }
    public decimal? Deductible { get; set; }
    public Policy? Policy { get; set; }
    public ICollection<PolicyBenefitSnapshot> BenefitSnapshots { get; set; } = new List<PolicyBenefitSnapshot>();
}
