using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class NumberSequence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public NumberSequenceType SequenceType { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
    public bool IncludeYear { get; set; } = true;
    public NumberSequenceResetFrequency ResetFrequency { get; set; } = NumberSequenceResetFrequency.Yearly;
    public int? LastResetYear { get; set; }
}
