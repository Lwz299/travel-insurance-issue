namespace TravelInsurance.Application.Common.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TravelInsurance.Api";
    public string Audience { get; set; } = "TravelInsurance.Client";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int ResetTokenMinutes { get; set; } = 10;
}
