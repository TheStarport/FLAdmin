namespace Service.Services.RedisClientService;
using Common.Configuration;
using StackExchange.Redis;

public class RedisClientService : IRedisClient
{
	private readonly IDatabase _database;

	public RedisClientService(FLAdminConfiguration adminConfiguration)
	{
		var redis = ConnectionMultiplexer.Connect($"{adminConfiguration.Redis.HostName}:{adminConfiguration.Redis.Port}");

		_database = redis.GetDatabase();
	}

	public void HashSet(RedisKey key, HashEntry[] hashEntries) => _database.HashSet(key, hashEntries);
	public RedisValue HashGet(RedisKey key, RedisValue hashValue) => _database.HashGet(key, hashValue);

	public RedisValue[] HashGet(RedisKey key, RedisValue[] hashValues) => _database.HashGet(key, hashValues);

	public HashEntry[] HashGetAll(RedisKey key) => _database.HashGetAll(key);
}
