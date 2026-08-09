namespace TravelInsurance.Domain.Enums;

public enum WalletTransactionType
{
    TopUp = 1,
    Reservation = 2,
    ReservationReleased = 3,
    PolicyDebit = 4,
    CancellationCredit = 5,
    AdjustmentCredit = 6,
    AdjustmentDebit = 7
}
