namespace Logic.Auth;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Auth;
using Common.State.ModalInfo;
using Fluxor;
using Microsoft.Extensions.Logging;

public class PersistentRoleProvider : IPersistentRoleProvider
{
	private readonly ILogger<PersistentRoleProvider> _logger;
	private readonly IJwtProvider _provider;
	private readonly IDispatcher _dispatcher;
	private List<AdminUser> _users = new();
	private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FLAdmin", "users.json");

	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		Converters = { new JsonStringEnumConverter() },
		WriteIndented = true
	};

	public PersistentRoleProvider(ILogger<PersistentRoleProvider> logger, IJwtProvider provider, IDispatcher dispatcher)
	{
		_logger = logger;
		_provider = provider;
		_dispatcher = dispatcher;
		LoadUsers();
	}

	public AdminUser? GetUser(string name) => _users.Find(x => x.Name == name);

	public void LoadUsers()
	{
		try
		{
			FileInfo info = new(_path);
			if (!info.Directory!.Exists)
			{
				info.Directory.Create();
			}

			using var file = File.Open(_path, FileMode.OpenOrCreate) ?? throw new IOException("Unable to load or read users.json");

			_users = JsonSerializer.Deserialize<AdminUser[]>(file, JsonSerializerOptions)!.ToList();

			file.Close();

			if (_users.Find(user => user.Name is "Admin") is not null)
			{
				// Ensure the roles of the admin user are always up to date!
				UpdateRoles("Admin", Enum.GetValues<Role>());
			}
		}
		catch (Exception ex)
		{
			// It's bad, lets purge and start again.
			_logger.LogError(ex, "Unable to read user.json or was in wrong format.");
			GenerateDefaultAdminUserIfNotExists();
			File.WriteAllText(_path, JsonSerializer.Serialize(_users));
		}
	}

	public void SaveUsers()
	{
		try
		{
			var jsonString = JsonSerializer.Serialize(_users, JsonSerializerOptions);

			File.WriteAllText(_path, jsonString);
		}
		catch (Exception ex)
		{
			_dispatcher.Dispatch(new ModalInfoAction($"Unable to save users to disk", true));
			_logger.LogError(ex, "Unable to save users to user.json!");
		}
	}

	public void UpdateRoles(string name, IEnumerable<Role> roles)
	{
		var user = _users.Find(x => x.Name == name);
		if (user is null)
		{
			_dispatcher.Dispatch(new ModalInfoAction($"User ({name}) does not exist!", true));
			_logger.LogError("Unable to find user ({User}) when updating roles", name);
			return;
		}

		user.Roles = roles.ToArray();

		SaveUsers();
	}

	public void UpdateToken(string name, string token)
	{
		var user = _users.Find(x => x.Name == name);
		if (user is null)
		{
			_dispatcher.Dispatch(new ModalInfoAction($"User ({name}) does not exist", true));
			_logger.LogError("Unable to find user ({User}) when updating token", name);
			return;
		}

		user.Token = token;

		SaveUsers();
	}

	public void AddUser(AdminUser user)
	{
		var instance = _users.Find(x => x.Name == user.Name);
		if (instance is not null)
		{
			_dispatcher.Dispatch(new ModalInfoAction($"User ({user.Name}) already exists!", true));
			_logger.LogError("User {User} already exists!", user.Name);
			return;
		}

		_users.Add(user);
		SaveUsers();
	}

	public void RemoveUser(string name)
	{
		var user = _users.Find(x => x.Name == name);
		if (user is null)
		{
			_dispatcher.Dispatch(new ModalInfoAction($"User ({name}) does not exist", true));
			_logger.LogError("Unable to find user ({User}) when removing", name);
			return;
		}

		_users.Remove(user);
		SaveUsers();
	}

	public bool GenerateDefaultAdminUserIfNotExists()
	{
		if (_users.Find(user => user.Name is "Admin") is not null)
		{
			return false;
		}

		var admin = new AdminUser()
		{
			Name = "Admin",
			Roles = Enum.GetValues<Role>(),
			Token = _provider.GenerateToken("Admin")
		};

		_users.Add(admin);

		Task.Run(async () =>
		{
			// Delay one second before displaying notification 
			await Task.Delay(TimeSpan.FromSeconds(1));
			_dispatcher.Dispatch(new ModalInfoAction(
				"Admin user created! You can login with the following token (make sure to save it!): \n\n" +
				admin.Token));
		});
		SaveUsers();
		return true;
	}

	// Get all users back but remove their tokens
	public IEnumerable<AdminUser> GetUsers() => _users.Select(x =>
	{
		x.Token = "";
		return x;
	});
}