using Common.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Business.Auth;

public class JwtAuthStateProvider : AuthStateProvider
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IPersistantRoleProvider _roleProvider;

    public JwtAuthStateProvider(IJwtProvider jwtProvider, IPersistantRoleProvider roleProvider)
    {
        _jwtProvider = jwtProvider;
        _roleProvider = roleProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        return await Task.FromResult(new AuthenticationState(user));
    }

    public override bool Authenticate(string jwt)
    {
        var decryptedToken = _jwtProvider.DecryptToken(jwt);
        if (decryptedToken is null)
        {
            return false;
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(decryptedToken), "jwt");
        var user = new ClaimsPrincipal(identity);

        if (!user.Claims.Any())
        {
            return false;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        return true;
    }

    public override async Task<string> RegenerateTokenAsync()
    {
        var state = await GetAuthenticationStateAsync();

        var token = _jwtProvider.GenerateToken(state.User!.Identity!.Name!);

        _roleProvider.UpdateToken(state.User.Identity.Name!, token);

        return token;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        if (!jwt.Contains('.'))
        {
            return Enumerable.Empty<Claim>();
        }

        var payload = jwt.Split('.')[1];
        try
        {
            List<Claim> claims = new();
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(payload)!;

            string name = keyValuePairs["unique_name"]?.ToString() ?? string.Empty;

            claims.Add(new Claim(ClaimTypes.Name, name));
            if (string.IsNullOrEmpty(claims.First().Value))
            {
                return Enumerable.Empty<Claim>();
            }

            var user = _roleProvider.GetUser(name);
            if (user is null)
            {
                return Enumerable.Empty<Claim>();
            }

            claims.AddRange(user.Roles.Select(x => new Claim(ClaimTypes.Role, x.ToString())));

            return claims;
        }
        catch (Exception)
        {
            return Enumerable.Empty<Claim>();
        }
    }

	public override void SignOut()
    {
        // Reset auth state
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
