namespace Common.Configuration;

using System.Text.Json;
using System.Text.Json.Serialization;

[Serializable]
// ReSharper disable once InconsistentNaming
public class FLAdminConfiguration
{
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
		AllowTrailingCommas = true,
		IncludeFields = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.Never
	};

	public static string ConfigPath => Path.Combine(Environment.GetFolderPath(
		Environment.SpecialFolder.LocalApplicationData), "FLAdmin", "configuration.json");

	public void Save()
	{
		if (!Directory.Exists(ConfigPath))
		{
			_ = Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
		}
		File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, SerializerOptions));
	}

	public static FLAdminConfiguration Load()
	{
		try
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FLAdmin", "configuration.json");
			if (File.Exists(path))
			{
				_instance = JsonSerializer.Deserialize<FLAdminConfiguration>(File.ReadAllText(path), SerializerOptions);

				if (_instance is null)
				{
					Console.WriteLine("Error while deserializing configuration. Assuming corrupted file. Regenerating.");
					_instance = new FLAdminConfiguration();
				}
			}
			else
			{
				_instance = new FLAdminConfiguration();
			}

			// Force a save to ensure we have correct permissions
			_instance.Save();

			return _instance;
		}
		catch (IOException ex)
		{
			Console.WriteLine("Unable to load FLAdminConfiguration. Error accessing file. EX: " + ex.Message);
			throw;
		}
	}

	private static FLAdminConfiguration? _instance;
	public static FLAdminConfiguration Get() => _instance ??= Load();
	public static void Reset(FLAdminConfiguration? newConfig) => _instance = newConfig ?? new FLAdminConfiguration();

	public LoggingSettings Logging { get; set; } = new();
	public FLServerSettings Server { get; set; } = new();
	public MessagingSettings Messaging { get; set; } = new();
	public MongoSettings Mongo { get; set; } = new();

	public void CopyConfigTo(FLAdminConfiguration config)
	{
		config.Logging = new LoggingSettings
		{
			EnableDebugLogs = Logging.EnableDebugLogs,
			LogFileFLAdmin = Logging.LogFileFLAdmin,
			LogFileFLHook = Logging.LogFileFLHook,
			FluentDOptions =
			{
				Enable = Logging.FluentDOptions.Enable,
				UnixSocket = Logging.FluentDOptions.UnixSocket,
				Host = Logging.FluentDOptions.Host,
				Port = Logging.FluentDOptions.Port,
			}
		};

		config.Messaging = new MessagingSettings
		{
			EnableMessaging = Messaging.EnableMessaging,
			Port = Messaging.Port,
			HostName = Messaging.HostName,
			Username = Messaging.Username,
		};

		config.Server = new FLServerSettings
		{
			AutoDownloadLatestFLHook = Server.AutoDownloadLatestFLHook,
			AutoStartFLServer = Server.AutoStartFLServer,
			CheckForFLHookUpdates = Server.CheckForFLHookUpdates,
			UseFLHook = Server.UseFLHook,
			FLHookRepositry = Server.FLHookRepositry,
			FreelancerPath = Server.FreelancerPath,
			LaunchArgs = Server.LaunchArgs,
			Port = Server.Port,
		};

		config.Mongo = new MongoSettings()
		{
			ConnectionString = Mongo.ConnectionString,
			Host = Mongo.Host,
			Port = Mongo.Port,
		};
	}
}
