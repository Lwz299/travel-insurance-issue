namespace TravelInsurance.Application.Common.Options;

public class SecurityOptions
{
    public const string SectionName = "Security";

    public int RefreshTokenDays { get; set; } = 30;
    public int OtpMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 5;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
}
