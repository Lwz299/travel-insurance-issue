using System.Security.Cryptography;
using System.Text;
using TravelInsurance.Application.Authentication.Abstractions;

namespace TravelInsurance.Infrastructure.Security;

public class OtpService : IOtpService
{
    public string GenerateOtp() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    public string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }

    public bool VerifyOtp(string rawOtp, string otpHash) => HashOtp(rawOtp) == otpHash;
}
