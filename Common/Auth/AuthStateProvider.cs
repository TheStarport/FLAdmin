namespace Common.Auth;
using Microsoft.AspNetCore.Components.Authorization;

public abstract class AuthStateProvider : AuthenticationStateProvider
{
	public abstract bool Authenticate(string jwt);
	public abstract Task<string> RegenerateTokenAsync();
	public abstract void SignOut();
}
