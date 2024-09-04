namespace FlAdmin.Common.Services;

public interface IAuthService
{
    public Task<string?> Authenticate(string username, string password);
}