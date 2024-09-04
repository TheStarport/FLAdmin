using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Configs;
using FlAdmin.DataAccess;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlAdmin.Logic.Services;

public class CharacterService(IDatabaseAccess databaseAccess, FlAdminConfig config, ILogger<CharacterService> logger)
    : ICharacterService
{
    private readonly IMongoCollection<Account>
        _accounts = databaseAccess.GetDatabase().GetCollection<Account>(config.Mongo.AccountCollectionName);

    private readonly IMongoCollection<Character> _characters =
        databaseAccess.GetDatabase().GetCollection<Character>(config.Mongo.CharacterCollectionName);

    private readonly ILogger<CharacterService> _logger = logger;

    public List<Character> GetCharactersOfAccount(Account account)
    {
        List<Character> characterList = [];
        var filter = Builders<Character>.Filter.In(c => c.Id, account.Characters);
        characterList.AddRange(_characters.Find(filter).ToEnumerable());
        return characterList;
    }

    public Character? GetCharacterByName(string name)
    {
        return _characters.Find(character => character.CharacterName == name).FirstOrDefault();
    }

    public List<Character> QueryCharacters(IQueryable<Character> query)
    {
        throw new NotImplementedException();
    }

    public void DeleteAllCharactersOnAccount(string accountId)
    {
        var account = _accounts.Find(account => account.Id == accountId).FirstOrDefault();
        if (account is null) return;
        _characters.DeleteMany(character => character.AccountId == accountId);
        var update = Builders<Account>.Update.Set(a => a.Characters, []);
        var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);
        _accounts.UpdateOne(filter, update);
    }

    public void DeleteCharacter(string name)
    {
        _characters.DeleteOne(character => character.CharacterName == name);
    }

    public void DeleteCharacter(ObjectId id)
    {
        _characters.DeleteOne(character => character.Id == id);
    }

    public void MoveCharacter(ObjectId id, string newAccountId)
    {
        var character = _characters.Find(character => character.Id == id).FirstOrDefault();
        if (character is null) return;
        var oldAccount = _accounts.Find(account => account.Id == character.AccountId).FirstOrDefault();
        if (oldAccount is null) return;
        var newAccount = _accounts.Find(account => account.Id == newAccountId).FirstOrDefault();
        if (newAccount is null) return;
        oldAccount.Characters.Remove(id);
        newAccount.Characters.Add(id);

        var charFilter = Builders<Character>.Filter.Eq(c => c.Id, character.Id);
        var oldAccountFilter = Builders<Account>.Filter.Eq(a => a.Id, oldAccount.Id);
        var newAccountFilter = Builders<Account>.Filter.Eq(c => c.Id, newAccount.Id);

        var charUpdate = Builders<Character>.Update.Set(c => c.AccountId, newAccount.Id);
        var oldAccountUpdate = Builders<Account>.Update.Set(a => a.Characters, oldAccount.Characters);
        var newAccountUpdate = Builders<Account>.Update.Set(a => a.Characters, newAccount.Characters);

        _characters.UpdateOne(charFilter, charUpdate);
        _accounts.UpdateOne(oldAccountFilter, oldAccountUpdate);
        _accounts.UpdateOne(newAccountFilter, newAccountUpdate);
    }

    public void MoveCharacter(string name, string newAccountId)
    {
        var character = _characters.Find(character => character.CharacterName == name).FirstOrDefault();
        if (character is null) return;
        MoveCharacter(character.Id, newAccountId);
    }

    public void UpdateCharacter(Character character)
    {
        var filter = Builders<Character>.Filter.Eq(c => c.Id, character.Id);
        _characters.ReplaceOne(filter, character);
    }

    public void UpdateFieldOnCharacter(ObjectId id, BsonElement bsonElement)
    {
        var character = _characters.Find(c => c.Id == id).FirstOrDefault().ToBsonDocument();
        if (character is null) return;

        var pair = character.Elements.FirstOrDefault(field => field.Name == bsonElement.Name);
        if (pair.Value.GetType != bsonElement.Value.GetType) return;
        character.SetElement(bsonElement);
        var filter = Builders<Character>.Filter.Eq(a => a.Id, id);

        _characters.UpdateOne(filter, character);
    }
}