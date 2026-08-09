namespace TravelInsurance.Domain.Enums;

public enum QuoteStatus
{
    Draft = 1,
    Calculated = 2,
    Sent = 3,
    Accepted = 4,
    Paid = 5,
    ConvertedToPolicy = 6,
    Expired = 7,
    Cancelled = 8
}
