namespace Common.Storage;

using MongoDB.Driver;

public interface IMongoManager
{
	Task<bool> ConnectAsync();
	IMongoDatabase? LoadDatabase(string database);
	IMongoDatabase? GetDatabase();

	Task<IMongoCollection<T>?> GetCollectionAsync<T>(string collectionName, bool createIfNotExists = true);
}
