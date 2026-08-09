using TravelInsurance.Application.Authentication.Abstractions;

namespace TravelInsurance.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
