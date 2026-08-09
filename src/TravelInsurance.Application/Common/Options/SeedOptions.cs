namespace TravelInsurance.Application.Common.Options;

public class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; } = true;
    public string DefaultPassword { get; set; } = "Test@123456";
}
