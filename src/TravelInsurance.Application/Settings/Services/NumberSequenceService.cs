using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Settings.Services;

public class NumberSequenceService(INumberSequenceRepository repository) : INumberSequenceService
{
    public async Task<string> NextAsync(Guid companyId, NumberSequenceType type, CancellationToken cancellationToken = default)
    {
        var sequence = await repository.GetAsync(companyId, type, cancellationToken) ?? CreateDefault(companyId, type);
        if (sequence.CurrentValue == 0) repository.Add(sequence);
        var year = DateTime.UtcNow.Year;
        if (sequence.ResetFrequency == NumberSequenceResetFrequency.Yearly && sequence.LastResetYear != year) { sequence.CurrentValue = 0; sequence.LastResetYear = year; }
        sequence.CurrentValue++;
        await repository.SaveChangesAsync(cancellationToken);
        var yearPart = sequence.IncludeYear ? $"-{year}" : string.Empty;
        return $"{sequence.Prefix}{yearPart}-{sequence.CurrentValue:000000}".ToUpperInvariant();
    }

    private static NumberSequence CreateDefault(Guid companyId, NumberSequenceType type) => new() { CompanyId = companyId, SequenceType = type, Prefix = type switch { NumberSequenceType.Policy => "POL", NumberSequenceType.Quote => "QTE", NumberSequenceType.Claim => "CLM", NumberSequenceType.Payment => "PAY", NumberSequenceType.Bordereau => "BOR", NumberSequenceType.Settlement => "SET", _ => "RIN" } };
}

