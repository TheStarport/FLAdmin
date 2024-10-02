using EphemeralMongo;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Logic.DataAccess;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlAdmin.Tests;

public class EphemeralTestDatabase : IDisposable
{
    private IMongoRunner _mongoRunner;

    private const int Port = 12345;

    public readonly FlAdminConfig Config;
    public readonly IDatabaseAccess DatabaseAccess;

    public EphemeralTestDatabase()
    {
        var options = new MongoRunnerOptions()
        {
            DataDirectory = "./TestData",
            StandardErrorLogger = Console.WriteLine,
            AdditionalArguments = "--quiet",
            KillMongoProcessesWhenCurrentProcessExits = true,
            MongoPort = Port
        };
        
        _mongoRunner = MongoRunner.Run(options);
        
        Config = new FlAdminConfig()
        {
            Mongo = new MongoConfig()
            {
                ConnectionString = "mongodb://localhost:" + Port
            }
        };
        
        DatabaseAccess = new MongoDatabaseAccess(Config, new NullLogger<MongoDatabaseAccess>());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _mongoRunner.Dispose();
        Directory.Delete("./TestData", true);
    }
}