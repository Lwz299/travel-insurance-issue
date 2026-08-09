namespace TravelInsurance.Domain.Enums;

public enum CreditTransactionType
{
    CreditReserved = 1,
    ReservationReleased = 2,
    PolicyIssued = 3,
    PolicyCancelled = 4,
    PaymentReceived = 5,
    CreditNote = 6,
    DebitAdjustment = 7
}
