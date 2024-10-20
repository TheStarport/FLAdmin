using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Bogus;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Service.Extensions;
using MongoDB.Bson;
using System.Collections.Generic;
using LanguageExt;


namespace FlAdmin.Tests.DataAccessMocks;

public class AccountDataAccessMock : IAccountDataAccess, IDisposable
{
    List<Account> _accounts;

    public AccountDataAccessMock()
    {
        _accounts = GenerateRandomAccounts();
    }


    public Task<Option<AccountError>> CreateAccounts(params Account[] accounts)
    {
        return Task.FromResult<Option<AccountError>>(accounts.Any(a => _accounts.Any(x => x.Id == a.Id))
            ? AccountError.AccountIdAlreadyExists
            : new AccountError());
    }

    public Task<Option<AccountError>> UpdateAccount(BsonDocument account)
    {
        var accountId = account.GetValue("_id").AsString;
        if (accountId is null || accountId.Length is 0)
            return Task.FromResult<Option<AccountError>>(AccountError.AccountIdIsNull);

        if (_accounts.All(x => x.Id != accountId))
        {
            return Task.FromResult<Option<AccountError>>(AccountError.AccountNotFound);
        }

        return Task.FromResult(new Option<AccountError>());
    }

    public Task<Option<AccountError>> DeleteAccounts(params string[] ids)
    {
        if (ids.ToList().Any(_ => ids.Contains("SuperAdmin")))
        {
            return Task.FromResult<Option<AccountError>>(AccountError.AccountIsProtected);
        }

        return ids.Any(id => _accounts.Any(x => x.Id == id))
            ? Task.FromResult<Option<AccountError>>(AccountError.AccountNotFound)
            : Task.FromResult(new Option<AccountError>());
    }

    public Task<Either<AccountError, Account>> GetAccount(string accountId)
    {
        var account = _accounts.FirstOrDefault(x => x.Id == accountId);
        return account is null
            ? Task.FromResult<Either<AccountError, Account>>(AccountError.AccountNotFound)
            : Task.FromResult<Either<AccountError, Account>>(account);
    }

    public Task<Option<AccountError>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<AccountError>>(AccountError.FieldIsProtected);
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return Task.FromResult<Option<AccountError>>(AccountError.AccountIsProtected);
        }

        var account = _accounts.FirstOrDefault(x => x.Id == accountId);
        if (account is null)
        {
            return Task.FromResult<Option<AccountError>>(AccountError.AccountNotFound);
        }

        var doc = account.ToBsonDocument();
        try
        {
            var element = doc[fieldName];
            if (element.GetType() != value!.GetType())
            {
                return Task.FromResult<Option<AccountError>>(AccountError.ElementTypeMismatch);
            }
        }
        catch (KeyNotFoundException ex)
        {
            return Task.FromResult<Option<AccountError>>(AccountError.FieldDoesNotExist);
        }

        return Task.FromResult(new Option<AccountError>());
    }

    public Task<Option<AccountError>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<AccountError>>(AccountError.FieldIsProtected);
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return Task.FromResult<Option<AccountError>>(AccountError.AccountIsProtected);
        }

        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            return Task.FromResult<Option<AccountError>>(AccountError.AccountNotFound);
        }

        var doc = account.ToBsonDocument();
        doc.TryGetValue(fieldName, out var element);
        return element is not null
            ? Task.FromResult<Option<AccountError>>(AccountError.FieldAlreadyExists)
            : Task.FromResult(new Option<AccountError>());
    }

    public Task<Option<AccountError>> RemoveFieldOnAccount(string accountId, string fieldName)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<AccountError>>(AccountError.FieldIsProtected);
        }

        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            return Task.FromResult<Option<AccountError>>(AccountError.AccountNotFound);
        }

        var doc = account.ToBsonDocument();
        doc.TryGetValue(fieldName, out var element);
        return element is null
            ? Task.FromResult<Option<AccountError>>(AccountError.FieldDoesNotExist)
            : Task.FromResult(new Option<AccountError>());
    }


    public Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, int page = 1,
        int pageSize = 100)
    {
        var func = filter.Compile();
        var accounts = _accounts.Filter(func).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(accounts);
    }

    public Task<Option<AccountError>> ReplaceAccount(Account account)
    {
        return _accounts.All(x => x.Id != account.Id)
            ? Task.FromResult<Option<AccountError>>(AccountError.AccountNotFound)
            : Task.FromResult(new Option<AccountError>());
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

    private static System.Collections.Generic.HashSet<string> GenerateRandomWebRoles(string? username)
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

    public void Dispose()
    {
        _accounts.Clear();
    }
}