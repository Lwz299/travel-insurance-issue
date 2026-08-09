namespace TravelInsurance.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Paid = 3,
    PartiallyPaid = 4,
    Failed = 5,
    Cancelled = 6,
    Expired = 7,
    Refunded = 8,
    PartiallyRefunded = 9
}
