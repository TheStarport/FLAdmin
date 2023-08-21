namespace Service.Services.RedisClientService;
using StackExchange.Redis;

public interface IRedisClient
{
	void HashSet(RedisKey key, HashEntry[] hashEntries);
	RedisValue HashGet(RedisKey key, RedisValue hashValue);
	RedisValue[] HashGet(RedisKey key, RedisValue[] hashValues);
	HashEntry[] HashGetAll(RedisKey key);
}

