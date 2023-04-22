namespace Common.Auth;
public interface IJwtProvider
{
	string GenerateToken(string name);
	string? DecryptToken(string token);
}
