using LogiMarket.Application.Ports.Security;

namespace LogiMarket.Infrastructure.Security;

public class AuthService : IAuthService
{
    public Task<bool> VerifyPasswordAsync(string password, string hash)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, hash));
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}