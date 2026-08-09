namespace TravelInsurance.Domain.Entities;

public class AgencyCreditAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgencyCompanyId { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal CreditLimit { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal ReservedAmount { get; set; }
    public int BillingDay { get; set; } = 1;
    public int PaymentDueDays { get; set; } = 30;
    public int GracePeriodDays { get; set; }
    public bool AutoSuspendOnOverdue { get; set; } = true;
    public bool IsSuspended { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<AgencyCreditTransaction> Transactions { get; set; } = new List<AgencyCreditTransaction>();
}
