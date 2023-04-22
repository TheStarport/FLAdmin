namespace Business.Auth;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Auth;
using Microsoft.Extensions.Logging;

public class PersistantRoleProvider : IPersistantRoleProvider
{
	private readonly ILogger<PersistantRoleProvider> _logger;
	private readonly IJwtProvider _provider;
	private List<AdminUser> _users = new();
	private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FLAdmin", "users.json");

	public PersistantRoleProvider(ILogger<PersistantRoleProvider> logger, IJwtProvider provider)
	{
		_logger = logger;
		_provider = provider;
		LoadUsers();
	}

	public AdminUser? GetUser(string name) => _users.FirstOrDefault(x => x.Name == name);

	public void LoadUsers()
	{
		try
		{
			FileInfo info = new(_path);
			if (!info.Directory!.Exists)
			{
				info.Directory.Create();
			}

			using var file = File.Open(_path, FileMode.OpenOrCreate);
			if (file is null)
			{
				throw new IOException("Unable to load or read users.json");
			}

			_users = JsonSerializer.Deserialize<AdminUser[]>(file, new JsonSerializerOptions()
			{
				Converters =
				{
					new JsonStringEnumConverter(),
				}
			})!.ToList();
			file.Close();

			if (!_users.Any())
			{
				GenerateDefaultAdminUser();
			}
			else
			{
				// Ensure the roles of the admin user are always up to date!
				UpdateRoles("Admin", Enum.GetValues<Role>());
			}
		}
		catch (Exception ex)
		{
			// It's bad, lets purge and start again.
			_logger.LogError(ex, "Unable to read user.json or was in wrong format.");
			GenerateDefaultAdminUser();
			File.WriteAllText(_path, JsonSerializer.Serialize(_users));
		}
	}

	public void SaveUsers()
	{
		try
		{
			var jsonString = JsonSerializer.Serialize(_users, new JsonSerializerOptions()
			{
				WriteIndented = true,
				Converters =
				{
					new JsonStringEnumConverter()
				}
			});

			File.WriteAllText(_path, jsonString);
		}
		catch (Exception)
		{
			// TODO: set blazor error state with fluxor#
			_logger.LogError("Unable to save users to user.json!");
		}
	}

	public void UpdateRoles(string name, IEnumerable<Role> roles)
	{
		var user = _users.Find(x => x.Name == name);
		if (user is null)
		{
			_logger.LogError("Unable to find user ({user}) when updating roles", name);
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
			_logger.LogError("Unable to find user ({user}) when updating token", name);
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
			// TODO: Fluxor modal!
			_logger.LogError("User {user} already exists!", user.Name);
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
			_logger.LogError("Unable to find user ({user}) when removing", name);
			return;
		}

		_users.Remove(user);
		SaveUsers();
	}

	private void GenerateDefaultAdminUser()
	{
		_users.Add(new AdminUser()
		{
			Name = "Admin",
			Roles = Enum.GetValues<Role>(),
			Token = _provider.GenerateToken("Admin")
		});

		SaveUsers();
	}
}
