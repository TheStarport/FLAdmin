namespace Service.Services.RedisClientService;

public interface IRedisClient
{
	void Connect(string configuration);
	bool SetValue(string key, object objectToStore, int dbIndex = 0);
	T? GetValue<T>(string key, int dbIndex = 0);
	void DeleteValue(string key, int dbIndex = 0);
}

