using System.Security.Cryptography;
using System.Text;
using Bogus;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Service.Extensions;
using MongoDB.Bson;

namespace FlAdmin.Tests;

public static class HelperFunctions
{
    public static List<Account> GenerateRandomAccounts()
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

    public static List<Character> GenerateRandomCharacters()
    {
        var characters = new List<Character>();

        var validCharacterGenerator = new Faker<Character>()
            .RuleFor(x => x.CharacterName, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.Id, _ => ObjectId.GenerateNewId())
            .RuleFor(x => x.Money, f => f.Random.Int(0, Int32.MaxValue - 1));


        var fixedCharacter = new Character()
        {
            CharacterName = "Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d2"),
            Money = 12345,
            AccountId = "123abc456"
        };

        var fixedCharacter2 = new Character()
        {
            CharacterName = "Mr_Trent",
            Id = new ObjectId("65d3fde10f019879e20193d2"),
            Money = 1234567,
            AccountId = "123abc456"
        };


        validCharacterGenerator.Generate(498).ToList().ForEach(x => characters.Add(x));
        characters.Add(fixedCharacter);
        characters.Add(fixedCharacter2);

        return characters;
    }


    public static List<Account> GenerateRandomAccounts(List<Character> characters)
    {
        var userAccountGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, _ => Guid.NewGuid().ToString())
            .RuleFor(a => a.Username, _ => null)
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, f => f.Date.Past(5))
            .RuleFor(a => a.WebRoles, (_, a) => GenerateRandomWebRoles(a.Username))
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (_, a) => TestSalter(a.PasswordHash))
            .RuleFor(a => a.ScheduledUnbanDate, _ => null);

        var bannedAccountGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, _ => Guid.NewGuid().ToString())
            .RuleFor(a => a.Username, _ => null)
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, f => f.Date.Past(5))
            .RuleFor(a => a.WebRoles, (_, a) => GenerateRandomWebRoles(a.Username))
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (_, a) => TestSalter(a.PasswordHash))
            .RuleFor(a => a.ScheduledUnbanDate, f => f.Date.Future(5));


        var webAccountGenerator = new Faker<Account>()
            .RuleFor(a => a.Id, _ => Guid.NewGuid().ToString())
            .RuleFor(a => a.Username, f => f.Person.UserName)
            .RuleFor(a => a.Cash, f => f.Random.Int(0, int.MaxValue - 1))
            .RuleFor(a => a.LastOnline, _ => null)
            .RuleFor(a => a.WebRoles, (_, a) => GenerateRandomWebRoles(a.Username))
            .RuleFor(a => a.PasswordHash,
                (f, a) => PasswordTestHasher(a.Username, f.Random.String2(10)))
            .RuleFor(a => a.Salt, (_, a) => TestSalter(a.PasswordHash))
            .RuleFor(a => a.ScheduledUnbanDate, _ => null);


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


        var fixedCharacter = characters.Find(x => x.CharacterName == "Chad_Games");
        var fixedCharacter2 = characters.Find(x => x.CharacterName == "Mr_Trent");

        fixedTestAccount.Characters.Add(fixedCharacter!.Id);
        fixedTestAccount.Characters.Add(fixedCharacter2!.Id);

        var characterIndex = 0;
        foreach (var acc in testAccounts)
        {
            int numOfCharactersForAccount = Random.Shared.Next(1, 4);
            for (var i = characterIndex; i < numOfCharactersForAccount + characterIndex; i++)
            {
                characters[i].AccountId = acc.Id;
                acc.Characters.Add(characters[i].Id);
            }

            characterIndex += numOfCharactersForAccount;
        }

        testAccounts.Add(fixedTestAccount);

        testAccounts.Add(superAdmin);
        testAccounts.AddRange(bannedAccountGenerator.Generate(40));
        testAccounts.AddRange(webAccountGenerator.Generate(9));

        return testAccounts;
    }

    public static System.Collections.Generic.HashSet<string> GenerateRandomWebRoles(string? username)
    {
        if (username is null)
        {
            return new System.Collections.Generic.HashSet<string>();
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

    public static string? PasswordTestHasher(string? username, string password)
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

    public static byte[]? TestSalter(string? username)
    {
        if (username is null) return null;

        Random rnd = new Random();
        byte[] b = new byte[32];
        rnd.NextBytes(b);
        return b;
    }
}