namespace Common.Storage;

using MongoDB.Driver;

public interface IMongoManager
{
	Task<bool> ConnectAsync();
	IMongoDatabase? GetDatabase(string database);

	Task<IMongoCollection<T>?> GetCollectionAsync<T>(string collectionName, bool createIfNotExists = true);
}
