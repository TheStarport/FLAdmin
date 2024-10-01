using MongoDB.Driver;

namespace FlAdmin.Common.DataAccess;

public interface IDatabaseAccess
{
    IMongoCollection<T> GetCollection<T>(string collectionName);

    MongoClient GetClient();
}