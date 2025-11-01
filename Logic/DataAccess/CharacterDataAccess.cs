using System.Linq.Expressions;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SharpDX.Win32;

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

    public async Task<Option<ErrorResult>> CreateCharacters(CancellationToken token, params Character[] characters)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            await _characters.InsertManyAsync(characters, cancellationToken: token);
            return new Option<ErrorResult>();
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
                        return new ErrorResult(FLAdminErrorCode.CharacterAlreadyExists,
                            "Character already exists on the database.");
                    }
                }

            logger.LogError(ex,
                "Encountered unexpected mongo database error when attempting to add a character to the database.");
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<Option<ErrorResult>> UpdateCharacter(BsonDocument character, CancellationToken token)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var characterId = character.GetValue("_id").AsObjectId;
            var filter = Builders<Character>.Filter.Eq(a => a.Id, characterId);
            var updateDoc = new BsonDocument
            {
                { "$set", character }
            };

            var result = await _characters.UpdateOneAsync(filter, updateDoc, cancellationToken: token);

            return result.ModifiedCount is 0
                ? new ErrorResult(FLAdminErrorCode.CharacterNotFound, $"{characterId} not found.")
                : new Option<ErrorResult>();
        }
        catch (MongoWriteException ex)
        {
            if (ex.InnerException is MongoBulkWriteException wx)
                return wx.WriteErrors[0].Code is (int)MongoErrorCodes.DuplicateKey
                    ? new ErrorResult(FLAdminErrorCode.CharacterNameIsTaken)
                    : new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");

            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when updating character {}",
                character.GetValue("_id").AsObjectId);
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Attempt to update character with Bson document that does not have an ObjectId");
            return new ErrorResult(FLAdminErrorCode.CharacterIdIsNull, "CharacterId of provided BSON document is null");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<Option<ErrorResult>> DeleteCharacters(CancellationToken token, params string[] characters)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var result = await _characters.DeleteManyAsync(character => characters.Contains(character.CharacterName),
                cancellationToken: token);

            //TODO: Find a way to get more detailed information of which characters were not deleted. 
            return result.DeletedCount != characters.Length
                ? new ErrorResult(FLAdminErrorCode.CharacterNotFound, "one of the characters provided does not exist.")
                : new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when attempting to delete characters");
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<Either<ErrorResult, Character>> GetCharacter(Either<ObjectId, string> characterName,
        CancellationToken token)
    {
        try
        {
            var charDocRes = await GetCharacterBsonDocument(characterName, token);

            if (charDocRes.IsNone)
                return new ErrorResult(FLAdminErrorCode.CharacterNotFound, $"{characterName} not found.");

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
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<Option<ErrorResult>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value, CancellationToken token)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var charDocRes = await GetCharacterBsonDocument(character, token);

            if (charDocRes.IsNone)
                return new ErrorResult(FLAdminErrorCode.CharacterNotFound, $"{character} not found.");

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
            if (existCheck is not null)
                return new ErrorResult(FLAdminErrorCode.CharacterFieldAlreadyExists,
                    $"The field {fieldName} already exists on {character}.");

            charDoc.Add(keyValuePair);
            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result =
                await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj, cancellationToken: token);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{Character} failed to add new field of name {fieldName} with value of {value}",
                    character,
                    fieldName, value);
                return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
            }

            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered an error when attempting to add {fieldName} with value of {value} to {character}",
                fieldName, value, character);
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<Option<ErrorResult>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value, CancellationToken token)
    {
        if (fieldName == "_id")
            return new ErrorResult(FLAdminErrorCode.CharacterFieldIsProtected, "_id cannot be updated arbitrarily.");

        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var charDocRes = await GetCharacterBsonDocument(character, token);

            if (charDocRes.IsNone)
                return new ErrorResult(FLAdminErrorCode.CharacterNotFound, $"{character} not found.");

            var charDoc = charDocRes.Match<BsonDocument>(
                bs => bs,
                null! as BsonDocument
            );

            if (charDoc[fieldName] is null)
                return new ErrorResult(FLAdminErrorCode.CharacterFieldDoesNotExist,
                    $"field {fieldName} does not exist on {character}.");

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
                return new ErrorResult(FLAdminErrorCode.CharacterElementTypeMismatch,
                    $"the provided type of {fieldName} does not match the expected type of {oldValuePair.Value.GetType()} (Provided type was {newValuePair.Value.GetType()}.");

            charDoc[newValuePair.Name] = newValuePair.Value;

            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result =
                await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj, cancellationToken: token);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{Character} failed to update field of name {fieldName} with value of {value}",
                    character,
                    fieldName, value);
                return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
            }

            return new Option<ErrorResult>();
        }
        catch (MongoWriteException ex)
        {
            if (ex.InnerException is MongoBulkWriteException wx)
                return wx.WriteErrors[0].Code is (int)MongoErrorCodes.DuplicateKey
                    ? new ErrorResult(FLAdminErrorCode.CharacterNameIsTaken)
                    : new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");

            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered an error when attempting to update {fieldName} with value of {value} to {character}",
                fieldName, value, character);
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to edit nonexistent field {fieldName} on character {character}",
                fieldName,
                character);
            return new ErrorResult(FLAdminErrorCode.CharacterFieldDoesNotExist,
                $"{fieldName} does not exist on {character}.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<Option<ErrorResult>> RemoveFieldOnCharacter(Either<ObjectId, string> character,
        string fieldName, CancellationToken token)
    {
        //We only protect against these two as they are both indexed, other fields should be protected at the service layer
        if (fieldName is "_id" or "characterName")
            return new ErrorResult(FLAdminErrorCode.CharacterFieldIsProtected,
                $"{fieldName} is protected and cannot be deleted.");

        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var charDocRes = await GetCharacterBsonDocument(character, token);

            if (charDocRes.IsNone)
                return new ErrorResult(FLAdminErrorCode.CharacterNotFound, $"{character} not found.");

            var charDoc = charDocRes.Match<BsonDocument>(
                bs => bs,
                null! as BsonDocument
            );

            if (charDoc[fieldName] is null)
            {
                logger.LogWarning("Attempted to remove nonexistent field {fieldName} from {character}", fieldName,
                    character);
                return new ErrorResult(FLAdminErrorCode.CharacterFieldDoesNotExist,
                    $"{fieldName} does not exist on {character}.");
            }

            charDoc.Remove(fieldName);
            var charObj = BsonSerializer.Deserialize<Character>(charDoc);
            var result =
                await _characters.ReplaceOneAsync(ch => ch.Id == charObj.Id, charObj, cancellationToken: token);
            if (result.ModifiedCount is 0)
            {
                logger.LogError(
                    "Encountered an unexpected mongo error when attempting to remove field {fieldName} from character {character}",
                    character,
                    fieldName);
                return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
            }

            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogWarning(ex,
                "Encountered an unexpected mongo error when attempting to remove field {fieldName} from character {character}",
                fieldName, character);
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to remove nonexistent field {fieldName} on character {character}",
                fieldName,
                character);
            return new ErrorResult(FLAdminErrorCode.DatabaseError, "Database op failed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return new ErrorResult(FLAdminErrorCode.RequestCancelled);
        }
    }

    public async Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter,
        CancellationToken token, int page = 1,
        int pageSize = 100)
    {
        try
        {
            var foundAccounts = (await _characters.FindAsync(filter, cancellationToken: token)).ToList();
            return foundAccounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database error when getting characters by specified filter of {}",
                filter);
            return [];
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return[];
        }
    }

    public async Task<List<Character>> GetCharactersByFilter(BsonDocument filter, CancellationToken token, int page = 1,
        int pageSize = 100)
    {
        try
        {
            var foundAccounts = (await _characters.FindAsync(filter, cancellationToken: token)).ToList();
            return foundAccounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database error when getting characters by specified filter of {}",
                filter);
            return [];
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return [];
        }
    }

    private async Task<Option<BsonDocument>> GetCharacterBsonDocument(Either<ObjectId, string> character,
        CancellationToken token)
    {
        try
        {
            return await character.MatchAsync(
                async x =>
                {
                    var doc = (await _characters.FindAsync(ch => ch.CharacterName == x, cancellationToken: token))
                        .FirstOrDefault();
                    return doc is null ? new Option<BsonDocument>() : doc.ToBsonDocument();
                },
                async x =>
                {
                    var doc = (await _characters.FindAsync(ch => ch.Id == x, cancellationToken: token))
                        .FirstOrDefault();
                    return doc is null ? new Option<BsonDocument>() : doc.ToBsonDocument();
                }
            );
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return [];
        }
    }
}