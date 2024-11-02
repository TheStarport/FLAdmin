using System.Linq.Expressions;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FlAdmin.Logic.DataAccess;

public class CharacterDataAccess(
    IDatabaseAccess databaseAccess,
    FlAdminConfig config,
    ILogger<CharacterDataAccess> logger)
    : ICharacterDataAccess
{
    private readonly MongoClient _client = databaseAccess.GetClient();

    private readonly IMongoCollection<Character> _characters =
        databaseAccess.GetCollection<Character>(config.Mongo.CharacterCollectionName);

    public async Task<Option<CharacterError>> CreateCharacters(params Character[] characters)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            await _characters.InsertManyAsync(characters);
            return new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            if (ex is MongoBulkWriteException wx)
            {
                if (wx.WriteErrors.Count is not 0)
                {
                    var writeError = wx.WriteErrors[0];

                    if (writeError.Code is 11000)
                    {
                        logger.LogWarning(ex,
                            "Attempt to add characters with ids/names that already exist on the database.");
                        return CharacterError.CharacterAlreadyExists;
                    }
                }
            }

            logger.LogError(ex,
                "Encountered unexpected mongo database error when attempting to add a character to the database.");
            return CharacterError.DatabaseError;
        }
    }

    public async Task<Option<CharacterError>> UpdateCharacter(BsonDocument character)
    {
        var characterId = character.GetValue("_id").AsObjectId;
        using var session = await _client.StartSessionAsync();
        try
        {
            var newAccount = BsonSerializer.Deserialize<Character>(character);
            var filter = Builders<Character>.Filter.Eq(a => a.Id, characterId);
            var updateDoc = Builders<Character>.Update.Set(acc => acc, newAccount);

            var result = await _characters.UpdateOneAsync(filter, updateDoc);

            return result.ModifiedCount is 0 ? CharacterError.CharacterNotFound : new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when updating character {}", characterId);
            return CharacterError.DatabaseError;
        }
    }

    public async Task<Option<CharacterError>> DeleteCharacters(params string[] characters)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var result = await _characters.DeleteManyAsync(character => characters.Contains(character.CharacterName));

            return result.DeletedCount is 0 ? CharacterError.CharacterNotFound : new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when attempting to delete characters");
            return CharacterError.DatabaseError;
        }
    }

    public Task<Either<CharacterError, Character>> GetCharacter(Either<ObjectId, string> characterName)
    {
        try
        {
            var charDoc = GetCharacterBsonDocument(characterName);
            if (charDoc is null)
            {
                logger.LogWarning("Character {characterName} not found", characterName);
                return Task.FromResult<Either<CharacterError, Character>>(CharacterError.CharacterNotFound);
            }

            return Task.FromResult<Either<CharacterError, Character>>(BsonSerializer.Deserialize<Character>(charDoc));
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Mongo exception thrown when attempting to get character of name {characterName}",
                characterName);
            return Task.FromResult<Either<CharacterError, Character>>(CharacterError.DatabaseError);
        }
    }

    public async Task<Option<CharacterError>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var charDoc = GetCharacterBsonDocument(character);
            if (charDoc is null)
            {
                return CharacterError.CharacterNotFound;
            }

            BsonElement keyValuePair;
            if (typeof(T) == typeof(int))
            {
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            }
            else if (typeof(T) == typeof(float))
            {
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            }
            else
            {
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value));
            }

            charDoc.TryGetValue(fieldName, out var existCheck);
            if (existCheck is not null)
            {
                return CharacterError.FieldAlreadyExists;
            }

            charDoc.Add(keyValuePair);
            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result = await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{Character} failed to add new field of name {fieldName} with value of {value}",
                    character,
                    fieldName, value);
                return CharacterError.DatabaseError;
            }

            return new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered an error when attempting to add {fieldName} with value of {value} to {character}",
                fieldName, value, character);
            return CharacterError.DatabaseError;
        }
    }

    public async Task<Option<CharacterError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value)
    {
        if (fieldName == "_id")
        {
            return CharacterError.FieldIsProtected;
        }

        using var session = await _client.StartSessionAsync();
        try
        {
            var charDoc = GetCharacterBsonDocument(character);
            if (charDoc is null)
            {
                return CharacterError.CharacterNotFound;
            }

            if (charDoc[fieldName] is null)
            {
                return CharacterError.FieldDoesNotExist;
            }

            BsonElement newValuePair;
            if (typeof(T) == typeof(int))
            {
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            }
            else if (typeof(T) == typeof(float))
            {
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            }
            else
            {
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value));
            }

            var oldValuePair = charDoc.Elements.FirstOrDefault(field => field.Name == newValuePair.Name);

            if (oldValuePair.Value.GetType() != newValuePair.Value.GetType())
            {
                return CharacterError.ElementTypeMismatch;
            }

            charDoc[newValuePair.Name] = newValuePair.Value;

            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result = await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{Character} failed to update field of name {fieldName} with value of {value}",
                    character,
                    fieldName, value);
                return CharacterError.DatabaseError;
            }

            return new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered an error when attempting to update {fieldName} with value of {value} to {character}",
                fieldName, value, character);
            return CharacterError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to edit nonexistent field {fieldName} on character {character}",
                fieldName,
                character);
            return CharacterError.FieldDoesNotExist;
        }
    }

    public async Task<Option<CharacterError>> RemoveFieldOnCharacter(Either<ObjectId, string> character,
        string fieldName)
    {
        if (fieldName is "_id" or "characterName")
        {
            return CharacterError.FieldIsProtected;
        }

        using var session = await _client.StartSessionAsync();
        try
        {
            var charDoc = GetCharacterBsonDocument(character);
            if (charDoc is null)
            {
                return CharacterError.CharacterNotFound;
            }

            if (charDoc[fieldName] is null)
            {
                logger.LogWarning("Attempted to remove nonexistent field {fieldName} from {character}", fieldName,
                    character);
                return CharacterError.FieldDoesNotExist;
            }

            charDoc.Remove(fieldName);
            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result = await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError(
                    "Encountered an unexpected mongo error when attempting to remove field {fieldName} from character {character}",
                    character,
                    fieldName);
                return CharacterError.DatabaseError;
            }

            return new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            logger.LogWarning(ex,
                "Encountered an unexpected mongo error when attempting to remove field {fieldName} from character {character}",
                fieldName, character);
            return CharacterError.DatabaseError;
        }
    }

    public async Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter, int page = 1,
        int pageSize = 100)
    {
        try
        {
            var foundAccounts = (await _characters.FindAsync(filter)).ToList();
            return foundAccounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database error when getting characters by specified filter of {}",
                filter);
            return [];
        }
    }

    private BsonDocument? GetCharacterBsonDocument(Either<ObjectId, string> character)
    {
        return character.Match<BsonDocument>(
            Left: x =>
            {
                var doc = _characters.Find(ch => ch.Id == x).FirstOrDefault();
                return doc?.ToBsonDocument()!;
            },
            Right: x => 
            {
                var doc = _characters.Find(ch => ch.CharacterName == x).FirstOrDefault();
                return doc?.ToBsonDocument()!;
            }
        );
    }
}