namespace TravelInsurance.Domain.Entities;

public class BenefitType
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Benefit> Benefits { get; set; } = new List<Benefit>();
}
