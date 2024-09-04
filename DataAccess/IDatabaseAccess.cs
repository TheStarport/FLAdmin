using MongoDB.Driver;

namespace FlAdmin.DataAccess;

public interface IDatabaseAccess
{
    IMongoDatabase GetDatabase();
}