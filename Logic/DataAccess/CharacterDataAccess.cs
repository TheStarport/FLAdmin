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
    private readonly IMongoCollection<Character> _characters =
        databaseAccess.GetCollection<Character>(config.Mongo.CharacterCollectionName);

    private readonly MongoClient _client = databaseAccess.GetClient();

    public async Task<Option<FLAdminError>> CreateCharacters(params Character[] characters)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            await _characters.InsertManyAsync(characters);
            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            if (ex is MongoBulkWriteException wx)
                if (wx.WriteErrors.Count is not 0)
                {
                    var writeError = wx.WriteErrors[0];

                    if (writeError.Code is 11000)
                    {
                        logger.LogWarning(ex,
                            "Attempt to add characters with ids/names that already exist on the database.");
                        return FLAdminError.CharacterAlreadyExists;
                    }
                }

            logger.LogError(ex,
                "Encountered unexpected mongo database error when attempting to add a character to the database.");
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Option<FLAdminError>> UpdateCharacter(BsonDocument character)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var characterId = character.GetValue("_id").AsObjectId;
            var filter = Builders<Character>.Filter.Eq(a => a.Id, characterId);
            var updateDoc = new BsonDocument
            {
                { "$set", character }
            };

            var result = await _characters.UpdateOneAsync(filter, updateDoc);

            return result.ModifiedCount is 0 ? FLAdminError.CharacterNotFound : new Option<FLAdminError>();
        }
        catch (MongoWriteException ex)
        {
            if (ex.InnerException is MongoBulkWriteException wx)
                return wx.WriteErrors[0].Code is (int)MongoErrorCodes.DuplicateKey
                    ? FLAdminError.CharacterNameIsTaken
                    : FLAdminError.DatabaseError;

            return FLAdminError.DatabaseError;
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when updating character {}",
                character.GetValue("_id").AsObjectId);
            return FLAdminError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Attempt to update character with Bson document that does not have an ObjectId");
            return FLAdminError.CharacterIdIsNull;
        }
    }

    public async Task<Option<FLAdminError>> DeleteCharacters(params string[] characters)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var result = await _characters.DeleteManyAsync(character => characters.Contains(character.CharacterName));

            //TODO: Find a way to get more detailed information of which characters were not deleted. 
            return result.DeletedCount != characters.Length
                ? FLAdminError.CharacterNotFound
                : new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when attempting to delete characters");
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Either<FLAdminError, Character>> GetCharacter(Either<ObjectId, string> characterName)
    {
        try
        {
            var charDocRes = await GetCharacterBsonDocument(characterName);

            if (charDocRes.IsNone) return FLAdminError.CharacterNotFound;

            var charDoc = charDocRes.Match<BsonDocument>(
                bs => bs,
                null! as BsonDocument
            );

            return BsonSerializer.Deserialize<Character>(charDoc);
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Mongo exception thrown when attempting to get character of name {characterName}",
                characterName);
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Option<FLAdminError>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var charDocRes = await GetCharacterBsonDocument(character);

            if (charDocRes.IsNone) return FLAdminError.CharacterNotFound;

            var charDoc = charDocRes.Match<BsonDocument>(
                bs => bs,
                null! as BsonDocument
            );

            BsonElement keyValuePair;
            if (typeof(T) == typeof(int))
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else if (typeof(T) == typeof(float))
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            charDoc.TryGetValue(fieldName, out var existCheck);
            if (existCheck is not null) return FLAdminError.CharacterFieldAlreadyExists;

            charDoc.Add(keyValuePair);
            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result = await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{Character} failed to add new field of name {fieldName} with value of {value}",
                    character,
                    fieldName, value);
                return FLAdminError.DatabaseError;
            }

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered an error when attempting to add {fieldName} with value of {value} to {character}",
                fieldName, value, character);
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Option<FLAdminError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value)
    {
        if (fieldName == "_id") return FLAdminError.CharacterFieldIsProtected;

        using var session = await _client.StartSessionAsync();
        try
        {
            var charDocRes = await GetCharacterBsonDocument(character);

            if (charDocRes.IsNone) return FLAdminError.CharacterNotFound;

            var charDoc = charDocRes.Match<BsonDocument>(
                bs => bs,
                null! as BsonDocument
            );

            if (charDoc[fieldName] is null) return FLAdminError.CharacterFieldDoesNotExist;

            BsonElement newValuePair;
            if (typeof(T) == typeof(int))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt32());
            else if (typeof(T) == typeof(float))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else if (typeof(T) == typeof(long))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            var oldValuePair = charDoc.Elements.FirstOrDefault(field => field.Name == newValuePair.Name);

            if (oldValuePair.Value.GetType() != newValuePair.Value.GetType())
                return FLAdminError.CharacterElementTypeMismatch;

            charDoc[newValuePair.Name] = newValuePair.Value;

            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result = await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{Character} failed to update field of name {fieldName} with value of {value}",
                    character,
                    fieldName, value);
                return FLAdminError.DatabaseError;
            }

            return new Option<FLAdminError>();
        }
        catch (MongoWriteException ex)
        {
            if (ex.InnerException is MongoBulkWriteException wx)
                return wx.WriteErrors[0].Code is (int)MongoErrorCodes.DuplicateKey
                    ? FLAdminError.CharacterNameIsTaken
                    : FLAdminError.DatabaseError;

            return FLAdminError.DatabaseError;
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered an error when attempting to update {fieldName} with value of {value} to {character}",
                fieldName, value, character);
            return FLAdminError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to edit nonexistent field {fieldName} on character {character}",
                fieldName,
                character);
            return FLAdminError.CharacterFieldDoesNotExist;
        }
    }

    public async Task<Option<FLAdminError>> RemoveFieldOnCharacter(Either<ObjectId, string> character,
        string fieldName)
    {
        //We only protect against these two as they are both indexed, other fields should be protected at the service layer
        if (fieldName is "_id" or "characterName") return FLAdminError.CharacterFieldIsProtected;

        using var session = await _client.StartSessionAsync();
        try
        {
            var charDocRes = await GetCharacterBsonDocument(character);

            if (charDocRes.IsNone) return FLAdminError.CharacterNotFound;

            var charDoc = charDocRes.Match<BsonDocument>(
                bs => bs,
                null! as BsonDocument
            );

            if (charDoc[fieldName] is null)
            {
                logger.LogWarning("Attempted to remove nonexistent field {fieldName} from {character}", fieldName,
                    character);
                return FLAdminError.CharacterFieldDoesNotExist;
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
                return FLAdminError.DatabaseError;
            }

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogWarning(ex,
                "Encountered an unexpected mongo error when attempting to remove field {fieldName} from character {character}",
                fieldName, character);
            return FLAdminError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to remove nonexistent field {fieldName} on character {character}",
                fieldName,
                character);
            return FLAdminError.CharacterFieldDoesNotExist;
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

    private async Task<Option<BsonDocument>> GetCharacterBsonDocument(Either<ObjectId, string> character)
    {
        return await character.MatchAsync(
            async x =>
            {
                var doc = (await _characters.FindAsync(ch => ch.CharacterName == x)).FirstOrDefault();
                return doc is null ? new Option<BsonDocument>() : doc.ToBsonDocument();
            },
            async x =>
            {
                var doc = (await _characters.FindAsync(ch => ch.Id == x)).FirstOrDefault();
                return doc is null ? new Option<BsonDocument>() : doc.ToBsonDocument();
            }
        );
    }
}