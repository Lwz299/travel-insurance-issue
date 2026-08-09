namespace TravelInsurance.Domain.Entities;

public class AgencyWallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgencyCompanyId { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Balance { get; set; }
    public decimal ReservedBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
