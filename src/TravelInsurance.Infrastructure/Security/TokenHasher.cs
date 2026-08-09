using System.Security.Cryptography;
using System.Text;
using TravelInsurance.Application.Authentication.Abstractions;

namespace TravelInsurance.Infrastructure.Security;

public class TokenHasher : ITokenHasher
{
    public string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
