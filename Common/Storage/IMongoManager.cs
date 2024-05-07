namespace Common.Storage;

using MongoDB.Driver;

public interface IMongoManager
{
	Task<bool> ConnectAsync();
	IMongoDatabase GetDatabase(string database);
	IMongoCollection<T> GetCollection<T>(string collection);
}
