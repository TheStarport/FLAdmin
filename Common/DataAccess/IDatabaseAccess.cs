using MongoDB.Driver;

namespace FlAdmin.Common.DataAccess;

public interface IDatabaseAccess
{
    IMongoDatabase GetDatabase();

    MongoClient GetClient();
}