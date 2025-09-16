using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlAdmin.Common.DataAccess;

public interface IDatabaseAccess
{
    IMongoCollection<T> GetCollection<T>(string collectionName);

    MongoClient GetClient();

    Task<Either<FLAdminError, Guid>> StartSession();

    Task<Option<FLAdminError>> EndSession(bool commit);

    Task<Either<FLAdminError, BsonDocument>> SubmitQuery(BsonDocument query, Guid sessionId);
}