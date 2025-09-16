using System.Security.Claims;
using FlAdmin.Common.Auth;
using FlAdmin.Common.Services;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.Services.Auth;

public class AuthService(IJwtProvider jwtProvider, IAccountService accountService, ILogger<AuthService> logger)
    : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<Option<string>> Authenticate(string username, string password, CancellationToken token)
    {
        var account = await accountService.GetAccountByUserName(token, username);
        return account.Match<Option<string>>(
            Left: error => new Option<string>(),
            Right: val =>
            {
                if (!PasswordHasher.VerifyPassword(password, val.PasswordHash!, val.Salt!)) return new Option<string>();

                var token = jwtProvider.GenerateToken((val.ToClaimsPrincipal().Identity as ClaimsIdentity)!);
                return token;
            }
        );
    }
}