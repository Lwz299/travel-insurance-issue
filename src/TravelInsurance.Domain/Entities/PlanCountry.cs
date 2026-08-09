namespace TravelInsurance.Domain.Entities;

public class PlanCountry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid CountryId { get; set; }

    // false = covered/included, true = explicitly excluded
    public bool IsExcluded { get; set; }

    public InsurancePlan? Plan { get; set; }
    public Country? Country { get; set; }
}
