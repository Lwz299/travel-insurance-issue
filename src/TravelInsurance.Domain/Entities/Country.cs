namespace TravelInsurance.Domain.Entities;

public class Country
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<PlanCountry> PlanCountries { get; set; } = new List<PlanCountry>();
    public ICollection<CoverageZoneCountry> CoverageZoneCountries { get; set; } = new List<CoverageZoneCountry>();
}
