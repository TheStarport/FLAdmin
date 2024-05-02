namespace Logic.Storage;
using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

public class MongoManager : IMongoManager
{
	private readonly ILogger<MongoManager> _logger;
	private readonly FLAdminConfiguration _configuration;
	private MongoClient? MongoClient { get; set; }
	private IMongoDatabase? Database { get; set; }

	public MongoManager(ILogger<MongoManager> logger, FLAdminConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	public async Task<bool> ConnectAsync()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(_configuration.Mongo.ConnectionString))
			{
				var clientSettings = new MongoClientSettings()
				{
					Scheme = ConnectionStringScheme.MongoDB,
					Server = new MongoServerAddress(_configuration.Mongo.Host, _configuration.Mongo.Port),
				};

				if (!string.IsNullOrEmpty(_configuration.Mongo.Username))
				{
					clientSettings.Credential = MongoCredential.CreateCredential(
						string.IsNullOrWhiteSpace(_configuration.Mongo.AuthDatabase) ? "admin" : _configuration.Mongo.AuthDatabase,
						_configuration.Mongo.Username,
						_configuration.Mongo.Password);
				}

				MongoClient = new MongoClient(clientSettings);
			}
			else if (!string.IsNullOrWhiteSpace(_configuration.Mongo.ConnectionString))
			{
				MongoClient = new MongoClient(_configuration.Mongo.ConnectionString);
			}
			else
			{
				return false;
			}

			Database = MongoClient.GetDatabase(_configuration.Mongo.PrimaryDatabaseName);
			_ = await Database.RunCommandAsync<BsonDocument>(new BsonDocument { { "ping", 1 } });
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Unable to create connection to MongoDB.");
			return false;
		}
	}

	public IMongoDatabase? LoadDatabase(string database) => MongoClient?.GetDatabase(database);
	public IMongoDatabase? GetDatabase() => Database;

	public async Task<IMongoCollection<T>?> GetCollectionAsync<T>(string collectionName, bool createIfNotExists = true)
	{
		if (Database is null)
		{
			return null;
		}

		if (!createIfNotExists)
		{
			return Database.GetCollection<T>(collectionName);
		}

		var filter = new BsonDocument("name", collectionName);
		var options = new ListCollectionNamesOptions { Filter = filter };

		if (!await (await Database.ListCollectionNamesAsync(options)).AnyAsync())
		{
			// collection does not exist, so we create it
			await Database.CreateCollectionAsync(collectionName);
		}

		return Database.GetCollection<T>(collectionName);
	}
}
