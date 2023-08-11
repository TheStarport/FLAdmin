namespace Common.Configuration;

using System.Text.Json;

[Serializable]
public class FLAdminConfiguration
{
	public void Save() => File.WriteAllText(Path.Combine(Environment.GetFolderPath(
		Environment.SpecialFolder.LocalApplicationData), "FLAdmin", "configuration.json"), JsonSerializer.Serialize<FLAdminConfiguration>(this, new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true
		}));

	public static FLAdminConfiguration Load()
	{
		try
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FLAdmin", "configuration.json");
			if (File.Exists(path))
			{
				_instance = JsonSerializer.Deserialize<FLAdminConfiguration>(File.ReadAllText(path), new JsonSerializerOptions()
				{
					PropertyNameCaseInsensitive = true,
					AllowTrailingCommas = true,
					IncludeFields = true
				});

				if (_instance is null)
				{
					Console.WriteLine("Error while deserializing configuration. Assuming corrupted file. Regenerating.");
					_instance = new FLAdminConfiguration();
					return _instance;
				}
			}
			else
			{
				_instance = new FLAdminConfiguration();
				_instance.Save();
			}

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

	public FLServerSettings Server { get; set; } = new();
	public MessagingSettings Messaging { get; set; } = new();
}
