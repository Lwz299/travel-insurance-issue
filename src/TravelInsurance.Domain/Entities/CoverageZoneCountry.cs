namespace TravelInsurance.Domain.Entities;

public class CoverageZoneCountry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CoverageZoneId { get; set; }
    public Guid CountryId { get; set; }

    // For regional zones false means included. For worldwide zones true means excluded.
    public bool IsExcluded { get; set; }

    public CoverageZone? CoverageZone { get; set; }
    public Country? Country { get; set; }
}
