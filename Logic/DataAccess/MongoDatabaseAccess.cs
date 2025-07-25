using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlAdmin.Logic.DataAccess;

public class MongoDatabaseAccess : IDatabaseAccess
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDatabaseAccess> _logger;


    private IClientSessionHandle? _session;
    private Guid _sessionId;

    public MongoDatabaseAccess(FlAdminConfig config, ILogger<MongoDatabaseAccess> logger)
    {
        _client = new MongoClient(config.Mongo.ConnectionString);
        _logger = logger;
        _database = _client.GetDatabase(config.Mongo.DatabaseName);
        _session = null;
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public MongoClient GetClient()
    {
        return _client;
    }

    public async Task<Either<FLAdminError, Guid>> StartSession()
    {
        if (_session is not null)
        {
            return FLAdminError.SessionAlreadyExists;
        }

        try
        {
            _session = await _client.StartSessionAsync();

            _sessionId = Guid.NewGuid();
            return _sessionId;
        }
        catch (MongoException ex)
        {
            return FLAdminError.SessionAlreadyExists;
        }
    }

    public async Task<Option<FLAdminError>> EndSession(bool commit)
    {
        if (_session is null)
        {
            //Make sure the sessionID is empty
            _sessionId = Guid.Empty;
            return FLAdminError.SessionNotStarted;
        }

        try
        {
            if (commit)
            {
                await _session.CommitTransactionAsync();
            }
            else
            {
                await _session.AbortTransactionAsync();
            }
            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            return FLAdminError.DatabaseError;
        }
        
    }


    public async Task<Either<FLAdminError, BsonDocument>> SubmitQuery(BsonDocument query, Guid sessionId)
    {
        if (_session is null)
        {
            return FLAdminError.SessionNotStarted;
        }

        if (sessionId == _sessionId)
        {
            return FLAdminError.SessionIdMismatch;
        }

        try
        {
            var res = await _database.RunCommandAsync<BsonDocument>(query);
            return res;
        }
        catch (MongoCommandException ex)
        {
            return FLAdminError.CommandError;
        }
    }
}