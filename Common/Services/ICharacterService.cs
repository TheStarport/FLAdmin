using FlAdmin.Common.Models.Database;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface ICharacterService
{
    List<Character> GetCharactersOfAccount(Account account);
    Character? GetCharacterByName(string name);
    List<Character> QueryCharacters(IQueryable<Character> query);

    void DeleteAllCharactersOnAccount(string accountId);
    void DeleteCharacter(string name);
    void DeleteCharacter(ObjectId id);

    void MoveCharacter(ObjectId id, string newAccountId);
    void MoveCharacter(string name, string newAccountId);


    void UpdateCharacter(Character character);
    void UpdateFieldOnCharacter(ObjectId id, BsonElement bsonElement);
}