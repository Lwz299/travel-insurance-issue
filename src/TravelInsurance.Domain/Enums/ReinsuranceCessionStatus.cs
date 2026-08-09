namespace TravelInsurance.Domain.Enums;

public enum ReinsuranceCessionStatus
{
    PendingBordereau = 1,
    PendingSubmission = 2,
    IncludedInBordereau = 3,
    Reported = 4,
    Cancelled = 5
}
