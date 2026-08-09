namespace TravelInsurance.Domain.Enums;

public enum IssuanceStatus
{
    Draft = 1,
    PendingValidation = 2,
    PendingPayment = 3,
    PendingCreditCheck = 4,
    PendingApproval = 5,
    Processing = 6,
    Issued = 7,
    Failed = 8,
    Rejected = 9,
    Cancelled = 10
}
