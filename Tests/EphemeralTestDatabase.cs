using EphemeralMongo;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Logic.DataAccess;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;

namespace FlAdmin.Tests;

public class EphemeralTestDatabase : IDisposable
{
    private const int Port = 12345;
    private readonly IFreelancerDataProvider _freelancerDataProvider;
    private readonly IMongoRunner _mongoRunner;

    public readonly FlAdminConfig Config;
    public readonly IDatabaseAccess DatabaseAccess;

    public EphemeralTestDatabase()
    {
        var options = new MongoRunnerOptions
        {
            DataDirectory = "./TestData",
            StandardErrorLogger = Console.WriteLine,
            AdditionalArguments = ["--quiet"],
            MongoPort = Port
        };


        _mongoRunner = MongoRunner.Run(options);

        Config = new FlAdminConfig
        {
            Mongo = new MongoConfig
            {
                ConnectionString = "mongodb://localhost:" + Port
            }
        };

        _freelancerDataProvider = new FreelancerDataProvider(new NullLogger<FreelancerDataProvider>(), Config);


        var client = new MongoClient(Config.Mongo.ConnectionString);
        client.DropDatabase(Config.Mongo.DatabaseName);
        var database = client.GetDatabase(Config.Mongo.DatabaseName);
        var accountCollection = database.GetCollection<Account>(Config.Mongo.AccountCollectionName);
        var characterCollection = database.GetCollection<Character>(Config.Mongo.CharacterCollectionName);

        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel =
            new CreateIndexModel<Character>(Builders<Character>.IndexKeys.Ascending(x => x.CharacterName),
                indexOptions);
        characterCollection.Indexes.CreateOne(indexModel);

        var testCharacters = HelperFunctions.GenerateRandomCharacters();
        var testAccounts = HelperFunctions.GenerateRandomAccounts(testCharacters);

        accountCollection.InsertMany(testAccounts);
        characterCollection.InsertMany(testCharacters);

        /*
        var jsonOptions = new JsonSerializerOptions {WriteIndented = true};
        string jsonString = JsonSerializer.Serialize(testAccounts, jsonOptions);
        */

        DatabaseAccess = new MongoDatabaseAccess(Config, new NullLogger<MongoDatabaseAccess>());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _mongoRunner.Dispose();
        Directory.Delete("./TestData", true);
    }
}