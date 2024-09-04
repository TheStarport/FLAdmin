using System.Security.Claims;
using FlAdmin.Common.Auth;
using FlAdmin.Common.Services;

namespace FlAdmin.Logic.Services.Auth;

public class AuthService(IJwtProvider jwtProvider, IAccountService accountService, ILogger<AuthService> logger)
    : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<string?> Authenticate(string username, string password)
    {
        var account = await accountService.GetAccountByUserName(username);
        if (account is null) return null;

        if (!PasswordHasher.VerifyPassword(password, account.PasswordHash!, account.Salt!)) return null;

        var token = jwtProvider.GenerateToken((account.ToClaimsPrincipal().Identity as ClaimsIdentity)!);
        return token;
    }
}