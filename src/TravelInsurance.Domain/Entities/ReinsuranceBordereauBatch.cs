using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class ReinsuranceBordereauBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReinsuranceAgreementId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public ReinsuranceBordereauStatus Status { get; set; } = ReinsuranceBordereauStatus.Generated;
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinalizedAt { get; set; }

    public ReinsuranceAgreement? ReinsuranceAgreement { get; set; }
    public ICollection<ReinsuranceCession> Cessions { get; set; } = new List<ReinsuranceCession>();
}
