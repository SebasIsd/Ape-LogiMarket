namespace LogiMarket.Application.Ports.Security;

public interface IAuthService
{
    Task<bool> VerifyPasswordAsync(string password, string hash);
    string HashPassword(string password);
}