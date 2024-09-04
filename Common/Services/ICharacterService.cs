using FlAdmin.Common.Models.Database;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface ICharacterService
{
    Task<List<Character>> GetCharactersOfAccount(Account account);
    Task<Character?> GetCharacterByName(string name);
    Task<List<Character>> QueryCharacters(IQueryable<Character> query);

    Task DeleteAllCharactersOnAccount(string accountId);
    Task DeleteCharacter(string name);
    Task DeleteCharacter(ObjectId id);

    Task MoveCharacter(ObjectId id, string newAccountId);
    Task MoveCharacter(string name, string newAccountId);


    Task UpdateCharacter(Character character);
    Task UpdateFieldOnCharacter(ObjectId id, BsonElement bsonElement);
}