namespace Service.Services.RedisClientService;
using Common.Configuration;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System.Text.Json;

public class RedisClientService : IRedisClient
{
	private readonly FLAdminConfiguration _adminConfiguration;

	private ConnectionMultiplexer? _connection;

	public RedisClientService(FLAdminConfiguration adminConfiguration) => _adminConfiguration = adminConfiguration;

	private void ConnectIfNecessary()
	{
		if (_connection is { IsConnected: true })
		{
			return;
		}

		Connect(string.Empty);
	}

	private JsonCommands GetJsonCommands(int dbIndex = 0)
	{
		ConnectIfNecessary();

		var db = _connection?.GetDatabase(dbIndex);

		return db!.JSON();
	}

	public void Connect(string configuration)
	{
		if (string.IsNullOrEmpty(configuration) || string.IsNullOrWhiteSpace(configuration))
		{
			_connection = ConnectionMultiplexer.Connect($"{_adminConfiguration.Redis.HostName}:{_adminConfiguration.Redis.Port}");

			return;
		}

		_connection = ConnectionMultiplexer.Connect(configuration);
	}

	public bool SetValue(string key, object objectToStore, int dbIndex = 0)
	{
		string serializedObject;

		try
		{
			serializedObject = JsonSerializer.Serialize(objectToStore, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			});
		}
		catch (Exception e)
		{
			Console.WriteLine($"Unable to serialize object. Reason: {e.Message}");
			throw;
		}

		return GetJsonCommands(dbIndex).Set(key,"$",serializedObject);
	}

	public T? GetValue<T>(string key, int dbIndex = 0) => GetJsonCommands(dbIndex).Get<T>(key);

	public void DeleteValue(string key, int dbIndex = 0) => GetJsonCommands(dbIndex).Del(key);
}
