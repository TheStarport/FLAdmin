using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlAdmin.Common.DataAccess;

public interface IDatabaseAccess
{
    IMongoCollection<T> GetCollection<T>(string collectionName);

    MongoClient GetClient();

    Task<Either<FLAdminErrorCode, Guid>> StartSession();

    Task<Option<FLAdminErrorCode>> EndSession(bool commit);

    Task<Either<FLAdminErrorCode, BsonDocument>> SubmitQuery(BsonDocument query, Guid sessionId);
}