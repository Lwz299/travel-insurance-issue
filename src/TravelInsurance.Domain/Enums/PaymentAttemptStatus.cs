namespace TravelInsurance.Domain.Enums;

public enum PaymentAttemptStatus
{
    Created = 1,
    Pending = 2,
    RequiresAction = 3,
    Succeeded = 4,
    Failed = 5,
    Cancelled = 6,
    Expired = 7
}
