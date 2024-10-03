using System.Security.Cryptography;
using Bogus;
using EphemeralMongo;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Logic.DataAccess;
using FlAdmin.Service.Extensions;
using LibreLancer;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using SharpDX.Text;
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


        var testAccountsGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, f => $"Value {f.UniqueIndex}")
            .RuleFor(a => a.Username, f => f.Person.UserName.OrNull(f, .9f))
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, f => f.Date.Past(5))
            .RuleFor(a => a.WebRoles, f => GenerateRandomWebRoles())
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (f, a) => TestSalter(a.PasswordHash));

        var testAccounts = testAccountsGenerator.Generate(150);
        try
        {
            var file = File.Create(@"C:\Projects\FlAdmin\Tests\SeedData/Accounts.json");
            using (var sw = new StreamWriter(file))
            {
                sw.Write(testAccounts);
                _mongoRunner.Import(Config.Mongo.DatabaseName, Config.Mongo.AccountCollectionName, @"C:\Projects\FlAdmin\Tests\SeedData\Accounts.json");
            }
            file.Close();
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
        }
       
        DatabaseAccess = new MongoDatabaseAccess(Config, new NullLogger<MongoDatabaseAccess>());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _mongoRunner.Dispose();
        Directory.Delete("./TestData", true);
    }

    //Helper method to randomly generate rules without superAdmin or no roles at all.
    private static HashSet<string> GenerateRandomWebRoles()
    {
        Role[] roles = {Role.Web, Role.ManageAccounts, Role.ManageAdmins, Role.ManageRoles, Role.ManageAutomation};
        var roleNames = new List<string>();

        var rnd = new Random();

        var returnEmpty = rnd.Next(0, 100);
        if (returnEmpty < 90) return [];

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
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(password)).ToString();
    }

    private byte[]? TestSalter(string? username)
    {
        if (username is null) return null;

        Random rnd = new Random();
        byte[] b = new byte[256];
        rnd.NextBytes(b);
        return b;
    }
}