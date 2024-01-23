namespace Common.Auth;
using System.Security.Claims;

public interface IJwtProvider
{
	string GenerateToken(ClaimsIdentity identity);
	string? DecryptToken(string token);
}
