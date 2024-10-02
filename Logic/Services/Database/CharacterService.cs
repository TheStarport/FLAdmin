using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlAdmin.Logic.Services.Database;

public class CharacterService(IDatabaseAccess databaseAccess, FlAdminConfig config, ILogger<CharacterService> logger)
    : ICharacterService
{
    private readonly IMongoCollection<Account>
        _accounts = databaseAccess.GetCollection<Account>(config.Mongo.AccountCollectionName);

    private readonly IMongoCollection<Character> _characters =
        databaseAccess.GetCollection<Character>(config.Mongo.CharacterCollectionName);

    private readonly MongoClient _client = databaseAccess.GetClient();
    private readonly ILogger<CharacterService> _logger = logger;

    public async Task<List<Character>> GetCharactersOfAccount(string accountId)
    {
        var account = (await _accounts.FindAsync(account => account.Id == accountId)).FirstOrDefault();
        List<Character> characterList = [];
        var filter = Builders<Character>.Filter.In(c => c.Id, account.Characters);
        var foundDoc = await _characters.FindAsync(filter);
        characterList.AddRange(foundDoc.ToEnumerable());
        return characterList;
    }

    public async Task<Character?> GetCharacterByName(string name)
    {
        var foundDoc = await _characters.FindAsync(character => character.CharacterName == name);
        return foundDoc.FirstOrDefault();
    }

    public async Task<List<Character>> QueryCharacters(IQueryable<Character> query)
    {
        throw new NotImplementedException();
    }

    public async Task AddCharacter(Character character)
    {
        if ((await _characters.FindAsync(doc => doc.CharacterName == character.CharacterName)).Any())
        {
            return;
        }

        await _characters.InsertOneAsync(character);
    }

    public async Task DeleteAllCharactersOnAccount(string accountId)
    {
        using var session = await _client.StartSessionAsync();

        try
        {
            var foundDoc = await _accounts.FindAsync(account => account.Id == accountId);
            var account = foundDoc.FirstOrDefault();
            if (account is null) return;
            await _characters.DeleteManyAsync(character => character.AccountId == accountId);
            var update = Builders<Account>.Update.Set(a => a.Characters, []);
            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);
            await _accounts.UpdateOneAsync(filter, update);
            await session.CommitTransactionAsync();
        }
        catch (MongoException e)
        {
            //TODO: Log and handle return codes appropriately. 
        }
    }

    public async Task DeleteCharacter(string name)
    {
        await _characters.DeleteOneAsync(character => character.CharacterName == name);
    }

    public async Task DeleteCharacter(ObjectId id)
    {
        await _characters.DeleteOneAsync(character => character.Id == id);
    }

    public async Task MoveCharacter(ObjectId id, string newAccountId)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            session.StartTransaction();

            var foundCharDoc = await _characters.FindAsync(character => character.Id == id);
            var character = foundCharDoc.FirstOrDefault();
            if (character is null) return;

            var foundOldAccountDoc = await _accounts.FindAsync(account => account.Id == character.AccountId);
            var oldAccount = foundOldAccountDoc.FirstOrDefault();
            if (oldAccount is null) return;


            var newAccountFoundDoc = await _accounts.FindAsync(account => account.Id == newAccountId);
            var newAccount = newAccountFoundDoc.FirstOrDefault();

            if (newAccount is null) return;


            oldAccount.Characters.Remove(id);
            newAccount.Characters.Add(id);

            var charFilter = Builders<Character>.Filter.Eq(c => c.Id, character.Id);
            var oldAccountFilter = Builders<Account>.Filter.Eq(a => a.Id, oldAccount.Id);
            var newAccountFilter = Builders<Account>.Filter.Eq(c => c.Id, newAccount.Id);


            var charUpdate = Builders<Character>.Update.Set(c => c.AccountId, newAccount.Id);
            var oldAccountUpdate = Builders<Account>.Update.Set(a => a.Characters, oldAccount.Characters);
            var newAccountUpdate = Builders<Account>.Update.Set(a => a.Characters, newAccount.Characters);

            await _characters.UpdateOneAsync(charFilter, charUpdate);
            await _accounts.UpdateOneAsync(oldAccountFilter, oldAccountUpdate);
            await _accounts.UpdateOneAsync(newAccountFilter, newAccountUpdate);

            await session.CommitTransactionAsync();
        }
        catch (MongoException ex)
        {
            //TODO: Log and catch
        }
    }

    public async Task MoveCharacter(string name, string newAccountId)
    {
        var foundCharDoc = await _characters.FindAsync(character => character.CharacterName == name);
        var character = foundCharDoc.FirstOrDefault();
        if (character is null) return;
        await MoveCharacter(character.Id, newAccountId);
    }

    public async Task UpdateCharacter(Character character)
    {
        var filter = Builders<Character>.Filter.Eq(c => c.Id, character.Id);
        await _characters.ReplaceOneAsync(filter, character);
    }

    public async Task UpdateFieldOnCharacter(string name, BsonElement bsonElement)
    {
        var foundCharDoc = await _characters.FindAsync(character => character.CharacterName == name);
        var character = foundCharDoc.FirstOrDefault().ToBsonDocument();

        if (character is null) return;

        var pair = character.Elements.FirstOrDefault(field => field.Name == bsonElement.Name);
        if (pair.Value.GetType != bsonElement.Value.GetType) return;
        character.SetElement(bsonElement);
        var filter = Builders<Character>.Filter.Eq(a => a.CharacterName, name);

        await _characters.UpdateOneAsync(filter, character);
    }
}