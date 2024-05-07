namespace Logic.Auth;

using System.Security.Claims;
using Blazored.LocalStorage;
using Common.Auth;
using Common.Models.Database;
using Common.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

public sealed class AuthStateProvider : AuthenticationStateProvider, IDisposable
{
	private const string StorageId = "fladminId";
	private const string StorageToken = "fladminToken";
	private readonly IAccountStorage _accountStorage;
	private readonly ILocalStorageService _localStorage;
	private readonly IJwtProvider _jwtProvider;
	private readonly ILogger<AuthStateProvider> _logger;

	public Account? CurrentUser { get; private set; }

	public AuthStateProvider(ILogger<AuthStateProvider> logger, IAccountStorage accountStorage, ILocalStorageService localStorage, IJwtProvider jwtProvider)
	{
		_accountStorage = accountStorage;
		_localStorage = localStorage;
		_jwtProvider = jwtProvider;
		_logger = logger;
		AuthenticationStateChanged += OnAuthenticationStateChangedAsync!;
	}

	public async Task<bool> Login(string username, string password, bool rememberMe)
	{
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			return false;
		}

		var principal = new ClaimsPrincipal();
		var account = (await _accountStorage.GetAccountAsync(account => account.Username == username));
		var success = false;

		if (account?.Salt is not null && PasswordHasher.VerifyPassword(password, account.PasswordHash, account.Salt))
		{
			principal = account.ToClaimsPrincipal();

			if (rememberMe)
			{
				var token = _jwtProvider.GenerateToken(principal.Identities.ElementAt(0));
				await _localStorage.SetItemAsStringAsync(StorageToken, token);
				await _localStorage.SetItemAsStringAsync(StorageId, account.Id);

				var salt = account.Salt;
				var hashedToken = PasswordHasher.GenerateSaltedHash(token, ref salt);
				await _accountStorage.SetAccountToken(account, hashedToken);
			}

			success = true;
		}

		CurrentUser = account;
		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
		return success;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var principal = new ClaimsPrincipal();
		var (id, token) = await GetTokenFromStorage();
		CurrentUser = null;

		if (token is null || id is null)
		{
			return new AuthenticationState(principal);
		}

		var decryptedToken = _jwtProvider.DecryptToken(token);
		if (decryptedToken is null) // Token invalid or expired
		{
			await _localStorage.ClearAsync();
			return new AuthenticationState(principal);
		}

		var account = await _accountStorage.GetAccountByIdAsync(id);
		if (account?.HashedToken is null || account.Salt is null) // Account not found
		{
			await _localStorage.ClearAsync();
			return new AuthenticationState(principal);
		}

		// Check that the token matched one in our database
		if (!PasswordHasher.VerifyPassword(token, account.HashedToken, account.Salt))
		{
			await _localStorage.ClearAsync();
			return new AuthenticationState(principal);
		}

		CurrentUser = account;
		principal = account.ToClaimsPrincipal();
		return new AuthenticationState(principal);
	}

	public async Task SignOut()
	{
		_logger.LogInformation("{Name} ({Id}) logging out", CurrentUser?.Username, CurrentUser?.Id);
		CurrentUser = null;

		await _localStorage.ClearAsync();
		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
	}

	public void Dispose() => AuthenticationStateChanged -= OnAuthenticationStateChangedAsync!;

	private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState?> task)
	{
		var authenticationState = await task;

		if (authenticationState is not null)
		{
			CurrentUser = Account.FromClaimsPrincipal(authenticationState.User);
		}
	}

	private async Task<(string? id, string? token)> GetTokenFromStorage()
	{
		try
		{
			var id = await _localStorage.GetItemAsStringAsync(StorageId);
			var token = await _localStorage.GetItemAsStringAsync(StorageToken);
			return (id, token);
		}
		catch
		{
			// When server side rendering is happening, cannot get client stuff :(
			return (null, null);
		}
	}
}
