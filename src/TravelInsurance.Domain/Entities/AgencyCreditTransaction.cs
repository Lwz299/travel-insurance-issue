using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class AgencyCreditTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CreditAccountId { get; set; }
    public Guid? PolicyId { get; set; }
    public Guid? IssuanceRequestId { get; set; }
    public Guid? SettlementId { get; set; }
    public CreditTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal OutstandingBefore { get; set; }
    public decimal OutstandingAfter { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public AgencyCreditAccount? CreditAccount { get; set; }
}
