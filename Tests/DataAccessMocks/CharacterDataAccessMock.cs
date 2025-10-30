using System.Linq.Expressions;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Tests.DataAccessMocks;

public class CharacterDataAccessMock : ICharacterDataAccess
{
    private readonly List<Account> _accounts;
    private readonly List<Character> _characters;

    public CharacterDataAccessMock()
    {
        _characters = HelperFunctions.GenerateRandomCharacters();
        _accounts = HelperFunctions.GenerateRandomAccounts(_characters);
    }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<Option<FLAdminErrorCode>> CreateCharacters(CancellationToken token, params Character[] characters)

    {
        return characters.Any(character =>
            _characters.Any(ch => ch.CharacterName == character.CharacterName || ch.Id == character.Id))
            ? FLAdminErrorCode.CharacterAlreadyExists
            : new Option<FLAdminErrorCode>();
    }

    public async Task<Option<FLAdminErrorCode>> UpdateCharacter(BsonDocument character, CancellationToken token)
    {
        if (!character.TryGetValue("_id", out var id)) return FLAdminErrorCode.CharacterIdIsNull;

        if (character.TryGetValue("characterName", out var name))
            if (name == "Mr_Trent" || name == "Chad_Games")
                return FLAdminErrorCode.CharacterNameIsTaken;

        return new Option<FLAdminErrorCode>();
    }

    public async Task<Option<FLAdminErrorCode>> DeleteCharacters(CancellationToken token, params string[] characters)
    {
        var found = false;

        foreach (var ch in characters)
            if (_characters.Any(c => c.CharacterName == ch))
            {
                found = true;
                break;
            }

        return !found ? FLAdminErrorCode.CharacterNotFound : new Option<FLAdminErrorCode>();
    }

    public async Task<Either<FLAdminErrorCode, Character>> GetCharacter(Either<ObjectId, string> characterName,
        CancellationToken token)
    {
        var val = characterName.Match<Option<Character>>(
            Left: ch => { return _characters.FirstOrDefault(x => x.Id == ch) ?? new Option<Character>(); },
            Right: ch => { return _characters.FirstOrDefault(x => x.CharacterName == ch) ?? new Option<Character>(); }
        );

        return val.Match<Either<FLAdminErrorCode, Character>>(
            None: () => FLAdminErrorCode.CharacterNotFound,
            Some: ch => ch
        );
    }

    public async Task<Option<FLAdminErrorCode>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value, CancellationToken token)
    {
        var val = character.Match<Character?>(
            Left: ch => { return _characters.FirstOrDefault(x => x.Id == ch); },
            Right: ch => { return _characters.FirstOrDefault(x => x.CharacterName == ch); }
        );
        if (val is null) return FLAdminErrorCode.CharacterNotFound;

        return fieldName is "_id" or "characterName" or "money"
            ? FLAdminErrorCode.CharacterFieldAlreadyExists
            : new Option<FLAdminErrorCode>();
    }

    public async Task<Option<FLAdminErrorCode>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value, CancellationToken token)
    {
        var val = character.Match<Character?>(
            Left: ch => { return _characters.FirstOrDefault(x => x.Id == ch); },
            Right: ch => { return _characters.FirstOrDefault(x => x.CharacterName == ch); }
        );
        if (val is null) return FLAdminErrorCode.CharacterNotFound;

        if (fieldName != "characterName" && fieldName != "money" && fieldName != "accountId")
            return FLAdminErrorCode.CharacterFieldDoesNotExist;

        if (fieldName == "characterName")
            if (value as string is "Mr_Trent" or "Chad_Games")
                return FLAdminErrorCode.CharacterNameIsTaken;

        return new Option<FLAdminErrorCode>();
    }

    public async Task<Option<FLAdminErrorCode>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName,
        CancellationToken token)
    {
        var val = character.Match<Character?>(
            Left: ch => { return _characters.FirstOrDefault(x => x.Id == ch); },
            Right: ch => { return _characters.FirstOrDefault(x => x.CharacterName == ch); }
        );
        if (val is null) return FLAdminErrorCode.CharacterNotFound;

        return new Option<FLAdminErrorCode>();
    }


    public async Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter,
        CancellationToken token, int page = 1,
        int pageSize = 100)
    {
        var func = filter.Compile();
        return _characters.Filter(func).Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    public async Task<List<Character>> GetCharactersByFilter(BsonDocument filter, CancellationToken token, int page = 1,
        int pageSize = 100)
    {
        throw new NotImplementedException();
    }


#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}