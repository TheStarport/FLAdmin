using System.Linq.Expressions;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FlAdmin.Logic.DataAccess;

public class CharacterDataAccess(
    IDatabaseAccess databaseAccess,
    FlAdminConfig config,
    ILogger<CharacterDataAccess> logger)
    : ICharacterDataAccess
{
    private readonly MongoClient _client = databaseAccess.GetClient();
    private readonly IMongoCollection<Account> _accounts = databaseAccess.GetCollection<Account>(config.Mongo.AccountCollectionName);
    private readonly IMongoCollection<Character> _characters = databaseAccess.GetCollection<Character>(config.Mongo.CharacterCollectionName);


    public async Task<Option<CharacterError>> CreateCharacters(params Character[] characters)
    {
        using var session = await _client.StartSessionAsync();
        try
        {

            
            return new Option<CharacterError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered unexpected mongo database error when attempting to add a character to the database.");
            return CharacterError.DatabaseError;
            
        }
    }

    public Task<Option<CharacterError>> UpdateCharacter(Character character)
    {
        throw new NotImplementedException();
    }

    public Task<Option<CharacterError>> DeleteCharacters(params Character[] characters)
    {
        throw new NotImplementedException();
    }

    public Task<Either<CharacterError, Character>> GetCharacterByName(string characterName)
    {
        throw new NotImplementedException();
    }

    public Task<Option<CharacterError>> CreateFieldOnCharacter<T>(Character character, string fieldName, T value)
    {
        throw new NotImplementedException();
    }

    public Task<Option<CharacterError>> UpdateFieldOnCharacter<T>(Character character, string fieldName, T value)
    {
        throw new NotImplementedException();
    }

    public Task<Option<CharacterError>> RemoveFieldOnCharacter(Character character, string fieldName)
    {
        throw new NotImplementedException();
    }

    public List<Character> GetCharactersByFilter(Expression<Func<Character, bool>> filter, int page = 1, int pageSize = 100)
    {
        throw new NotImplementedException();
    }
    
    
    
}