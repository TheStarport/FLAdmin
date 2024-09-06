﻿using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Configs;
using FlAdmin.DataAccess;
using FlAdmin.Logic.Services.Auth;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlAdmin.Logic.Services.Database;

public class AccountService(IDatabaseAccess databaseAccess, FlAdminConfig config, ILogger<AccountService> logger)
    : IAccountService
{
    private readonly IMongoCollection<Account>
        _accounts = databaseAccess.GetDatabase().GetCollection<Account>(config.Mongo.AccountCollectionName);

    private readonly ILogger<AccountService> _logger = logger;

    public async Task<List<Account>> GetAllAccounts()
    {
        var foundDoc = await _accounts.FindAsync(account => true);
        return foundDoc.ToList();
    }

    public List<Account> QueryAccounts(IQueryable<Account> query)
    {
        throw new NotImplementedException();
    }

    public async Task<Account?> GetAccountById(string id)
    {
        return (await _accounts.FindAsync(account => account.Id == id)).FirstOrDefault();
    }

    public async Task AddAccounts(params Account[] accounts)
    {
        await _accounts.InsertManyAsync(accounts);
    }

    public async Task UpdateAccount(Account account)
    {
        var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
        await _accounts.ReplaceOneAsync(filter, account);
    }

    public async Task DeleteAccounts(params string[] ids)
    {
        await _accounts.DeleteManyAsync(account => ids.Contains(account.Id));
    }

    public async Task UpdateFieldOnAccount(BsonElement bsonElement, string accountId)
    {
        var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();

        if (account is null) return;

        var pair = account.Elements.FirstOrDefault(field => field.Name == bsonElement.Name);
        if (pair.Value.GetType != bsonElement.Value.GetType) return;

        account.SetElement(bsonElement);
        var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);

        await _accounts.UpdateOneAsync(filter, account);
    }

    public List<Account> GetOnlineAccounts()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CreateWebMaster(LoginModel loginModel)
    {
        var name = loginModel.Username.Trim();
        if (await _accounts!.CountDocumentsAsync(Builders<Account>.Filter.Eq("username", name)) is not 0) return false;

        var password = loginModel.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(loginModel.Password, ref salt);
        var account = new Account
        {
            Id = ObjectId.GenerateNewId().ToString(),
            PasswordHash = hashedPass,
            Salt = salt,
            Username = name,
            WebRoles = []
        };
        await _accounts.InsertOneAsync(account);
        return true;
    }

    public async Task<Account?> GetAccountByUserName(string userName)
    {
        var foundDocument = await _accounts.FindAsync(account => account.Username == userName);
        var account = foundDocument.FirstOrDefault();
        //TODO: Log warning of account not having a password with a username existing.

        return account?.PasswordHash is null ? null : account;
    }

    public async Task<List<Account>> GetAccountsActiveAfterDate(DateTimeOffset date)
    {
        return (await _accounts.FindAsync(x => x.LastOnline >= date)).ToList();
    }

    public async Task AddRolesToAccount(string id, List<Role> roles)
    {
        var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
        var update = Builders<Account>.Update.AddToSetEach(a => a.WebRoles, roles.Select(x => x.ToString()).ToList());
        await _accounts.FindOneAndUpdateAsync(filter, update);
    }

    public async Task BanAccount(string id, TimeSpan? duration)
    {
        if (duration is null)
        {
            duration = TimeSpan.FromDays(109500);
        }

        var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
        var update = Builders<Account>.Update.Set(a => a.ScheduledUnbanDate, DateTimeOffset.Now + duration);
        await _accounts.FindOneAndUpdateAsync(filter, update);
    }

    public async Task UnBanAccount(string id)
    {
        var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
        var update = Builders<Account>.Update.Set(a => a.ScheduledUnbanDate, null);
        await _accounts.FindOneAndUpdateAsync(filter, update);
    }

    public async Task RemoveRolesFromAccount(string id, List<Role> roles)
    {
        var account = (await _accounts.FindAsync(account => account.Id == id)).FirstOrDefault();
        if (account is null) return;

        var roleStrList =  roles.Select(x => x.ToString()).ToList();
        account.WebRoles.RemoveAll(r => roleStrList.Exists(str => str == r));
        var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
        await _accounts.ReplaceOneAsync(filter, account);
    }
}