using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models;
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

    public async Task<Either<ErrorResult, Guid>> StartSession()
    {
        if (_session is not null) return new ErrorResult(FLAdminErrorCode.SessionAlreadyExists, "Session already exists");

        try
        {
            _session = await _client.StartSessionAsync();

            _sessionId = Guid.NewGuid();
            return _sessionId;
        }
        catch (MongoException ex)
        {
            return new ErrorResult(FLAdminErrorCode.CommandError, ex.Message);
        }
    }

    public async Task<Option<ErrorResult>> EndSession(bool commit)
    {
        if (_session is null)
        {
            //Make sure the sessionID is empty
            _sessionId = Guid.Empty;
            return new ErrorResult(FLAdminErrorCode.SessionNotStarted);
        }

        try
        {
            if (commit)
                await _session.CommitTransactionAsync();
            else
                await _session.AbortTransactionAsync();
            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            return new ErrorResult(FLAdminErrorCode.CommandError, ex.Message);
        }
    }


    public async Task<Either<ErrorResult, BsonDocument>> SubmitQuery(BsonDocument query, Guid sessionId)
    {
        if (_session is null) return new ErrorResult(FLAdminErrorCode.SessionNotStarted);

        if (sessionId != _sessionId) return new ErrorResult(FLAdminErrorCode.SessionIdMismatch);

        try
        {
            var res = await _database.RunCommandAsync<BsonDocument>(query);
            return res;
        }
        catch (MongoCommandException ex)
        {
            return new ErrorResult(FLAdminErrorCode.CommandError, ex.Message);
        }
    }
}