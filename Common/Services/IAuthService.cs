using LanguageExt;

namespace FlAdmin.Common.Services;

public interface IAuthService
{
    public Task<Option<string>> Authenticate(string username, string password, CancellationToken token);
}