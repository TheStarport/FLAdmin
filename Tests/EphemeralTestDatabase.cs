using System.Security.Cryptography;
using Bogus;
using EphemeralMongo;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Logic.DataAccess;
using FlAdmin.Service.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Encoding = System.Text.Encoding;


namespace FlAdmin.Tests;

public class EphemeralTestDatabase : IDisposable
{
    private const int Port = 12345;

    public readonly FlAdminConfig Config;
    public readonly IDatabaseAccess DatabaseAccess;
    private readonly IMongoRunner _mongoRunner;

    public EphemeralTestDatabase()
    {
        var options = new MongoRunnerOptions
        {
            DataDirectory = "./TestData",
            StandardErrorLogger = Console.WriteLine,
            AdditionalArguments = "--quiet",
            KillMongoProcessesWhenCurrentProcessExits = true,
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

        var client = new MongoClient(Config.Mongo.ConnectionString);
        client.DropDatabase(Config.Mongo.DatabaseName);
        var database = client.GetDatabase(Config.Mongo.DatabaseName);
        var accountCollection = database.GetCollection<Account>(Config.Mongo.AccountCollectionName);
        var testAccounts = GenerateRandomAccounts();
        accountCollection.InsertMany(testAccounts);
        
        
        
        

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


    private List<Character> GenerateRandomCharacters()
    {
        var characters = new List<Character>();

        //FLServer can sometimes generate invalid characters so we need to have tests account for both
        //Valid and invalid character states on the database. 
        var validCharacterGenerator = new Faker<Character>();
        
        
        
        return characters;
    }
    
    
    
    private List<Account> GenerateRandomAccounts()
    {
        var userAccountGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
            .RuleFor(a => a.Username, f => null)
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, f => f.Date.Past(5))
            .RuleFor(a => a.WebRoles, (f, a) => GenerateRandomWebRoles(a.Username))
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (f, a) => TestSalter(a.PasswordHash))
            .RuleFor(a => a.ScheduledUnbanDate, f => null);

        var bannedAccountGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
            .RuleFor(a => a.Username, f => null)
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, f => f.Date.Past(5))
            .RuleFor(a => a.WebRoles, (f, a) => GenerateRandomWebRoles(a.Username))
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (f, a) => TestSalter(a.PasswordHash))
            .RuleFor(a => a.ScheduledUnbanDate, f => f.Date.Future(5));


        var webAccountGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
            .RuleFor(a => a.Username, f => f.Person.UserName)
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, f => null)
            .RuleFor(a => a.WebRoles, (f, a) => GenerateRandomWebRoles(a.Username))
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (f, a) => TestSalter(a.PasswordHash))
            .RuleFor(a => a.ScheduledUnbanDate, f => null);


        var superAdmin = new Account
        {
            Id = "abc123456",
            Username = "SuperAdmin",
            PasswordHash = PasswordTestHasher("SuperAdmin", "SuperAdmin Password"),
            WebRoles = {Role.SuperAdmin.GetEnumDescription()},
            Salt = TestSalter("SuperAdmin")
        };

        var fixedTestAccount = new Account
        {
            Id = "123abc456",
            LastOnline = DateTime.Now - TimeSpan.FromDays(30),
            Cash = 12345678
        };

        var testAccounts = userAccountGenerator.Generate(99);

        testAccounts.Add(superAdmin);
        testAccounts.Add(fixedTestAccount);
        testAccounts.AddRange(bannedAccountGenerator.Generate(40));
        testAccounts.AddRange(webAccountGenerator.Generate(9));

        return testAccounts;
    }

//Helper method to randomly generate roles without superAdmin.
    private static HashSet<string> GenerateRandomWebRoles(string? username)
    {
        if (username is null)
        {
            return new HashSet<string>();
        }

        Role[] roles = {Role.Web, Role.ManageAccounts, Role.ManageAdmins, Role.ManageRoles, Role.ManageAutomation};
        var roleNames = new List<string>();

        var rnd = new Random();

        var ints = Enumerable.Range(0, roles.Length - 1)
            .Select(i => new Tuple<int, int>(rnd.Next(0, roles.Length - 1), i))
            .OrderBy(i => i.Item1)
            .Select(i => i.Item2).ToList();
        var randomIndex = rnd.Next(0, ints.Count - 1);
        for (var i = 0; i < randomIndex; i++) roleNames.Add(roles[i].GetEnumDescription());

        return roleNames.ToHashSet();
    }

    private static string? PasswordTestHasher(string? username, string password)
    {
        if (username is null)
        {
            return null;
        }

        //Using an old obsolete hashing algorithm since this is just test data. 
        using HashAlgorithm algorithm = SHA1.Create();
        var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash);
    }

    private byte[]? TestSalter(string? username)
    {
        if (username is null) return null;

        Random rnd = new Random();
        byte[] b = new byte[32];
        rnd.NextBytes(b);
        return b;
    }
}