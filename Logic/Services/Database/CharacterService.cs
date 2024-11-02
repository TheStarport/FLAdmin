using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace FlAdmin.Logic.Services.Database;

public class CharacterService : ICharacterService
{
    private readonly IAccountDataAccess _accountDataAccess;
    private readonly ICharacterDataAccess _characterDataAccess;
    private readonly ILogger<CharacterService> _logger;
    private readonly IValidationService _validation;


    public CharacterService(ICharacterDataAccess characterDataAccess, IAccountDataAccess accountDataAccess,
        IValidationService validator,
        FlAdminConfig config, ILogger<CharacterService> logger)
    {
        _characterDataAccess = characterDataAccess;
        _accountDataAccess = accountDataAccess;
        _logger = logger;
        _validation = validator;
    }

    public async Task<Either<CharacterError, List<Character>>> GetCharactersOfAccount(string accountId)
    {
        var characters = await _characterDataAccess.GetCharactersByFilter(x => x.AccountId == accountId);

        if (characters.Count is 0) return CharacterError.CharacterNotFound;

        return characters;
    }

    public async Task<Either<CharacterError, Character>> GetCharacterByName(string name)
    {
        return await _characterDataAccess.GetCharacter(name);
    }

    public Task<List<Character>> QueryCharacters(IQueryable<Character> query)
    {
        throw new NotImplementedException();
    }

    public async Task<Option<CharacterError>> AddCharacter(Character character)
    {
        if (_validation.ValidateCharacter(character) is false) return CharacterError.InvalidCharacter;

        return await _characterDataAccess.CreateCharacters(character);
    }

    public async Task<Option<CharacterError>> DeleteAllCharactersOnAccount(string accountId)
    {
        var characters = await _characterDataAccess.GetCharactersByFilter(x => x.AccountId == accountId);

        if (characters.Count is 0) return CharacterError.CharacterNotFound;

        var charNames = characters.Select(character => character.CharacterName).ToList();

        var result = await _characterDataAccess.DeleteCharacters(charNames.ToArray());
        if (result.IsSome) return result;


        List<ObjectId> cleanedCharacterList = new();
        var result2 = await _accountDataAccess.UpdateFieldOnAccount(accountId, "characters", cleanedCharacterList);

        return result2.IsSome ? CharacterError.AccountError : new Option<CharacterError>();
    }

    public async Task<Option<CharacterError>> DeleteCharacter(Either<ObjectId, string> character)
    {
        var charRes = await _characterDataAccess.GetCharacter(character);
        if (charRes.IsLeft)
            return charRes.Match(
                Left: err => err, Right
                //Right should never really be reached as we're checking it.
                : _ => CharacterError.InvalidCharacter);

        var ch = charRes.Match<Character>(
            Left: _ => null!,
            Right: ch => ch);


        var accountRes = await _accountDataAccess.GetAccount(ch.AccountId);
        if (accountRes.IsLeft)
            return accountRes.Match(
                Left: err => CharacterError.AccountError,
                Right: err => CharacterError.AccountError);

        var account = accountRes.Match<Account>(
            Left: _ => null!,
            Right: x => x);
        account.Characters.Remove(ch.Id);

        var res = await _characterDataAccess.DeleteCharacters(ch.CharacterName);
        if (res.IsSome) return res;

        var res2 = await _accountDataAccess.UpdateAccount(account.ToBsonDocument());
        return res2.IsSome ? CharacterError.AccountError : new Option<CharacterError>();
    }

    public async Task<Option<CharacterError>> MoveCharacter(Either<ObjectId, string> character, string newAccountId)
    {
        var charRes = await _characterDataAccess.GetCharacter(character);
        if (charRes.IsLeft)
            return charRes.Match(
                Left: err => err, Right
                //Right should never really be reached as we're checking it.
                : _ => CharacterError.InvalidCharacter);

        var ch = charRes.Match<Character>(
            Left: _ => null!,
            Right: ch => ch);
        var oldAccountRes = await _accountDataAccess.GetAccount(ch.AccountId);
        if (oldAccountRes.IsLeft)
            return oldAccountRes.Match(
                Left: _ => CharacterError.AccountError,
                Right: _ => CharacterError.AccountError);

        var oldAccount = oldAccountRes.Match<Account>(
            Left: _ => null!,
            Right: x => x);

        var newAccountRes = await _accountDataAccess.GetAccount(newAccountId);
        if (newAccountRes.IsLeft)
            return newAccountRes.Match(
                Left: _ => CharacterError.AccountError,
                Right: _ => CharacterError.AccountError);

        var newAccount = newAccountRes.Match<Account>(
            Left: _ => null!,
            Right: x => x);


        oldAccount.Characters.Remove(ch.Id);
        newAccount.Characters.Add(ch.Id);

        var result1 = await _characterDataAccess.UpdateFieldOnCharacter(character, "accountId", newAccountId);
        if (result1.IsSome) return result1;

        var result2 = await _accountDataAccess.UpdateFieldOnAccount(oldAccount.Id, "characters", oldAccount.Characters);
        if (result2.IsSome) return CharacterError.AccountError;

        var result3 = await _accountDataAccess.UpdateFieldOnAccount(newAccount.Id, "accounts", newAccount.Characters);
        if (result3.IsSome) return CharacterError.AccountError;

        return new Option<CharacterError>();
    }

    public async Task<Option<CharacterError>> UpdateCharacter(Character character)
    {
        if (_validation.ValidateCharacter(character) is false) return CharacterError.InvalidCharacter;

        return await _characterDataAccess.UpdateCharacter(character.ToBsonDocument());
    }

    public async Task<Option<CharacterError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value)
    {
        return await _characterDataAccess.UpdateFieldOnCharacter(character, fieldName, value);
    }

    public async Task<Option<CharacterError>> RemoveFieldOnCharacter(Either<ObjectId, string> character,
        string fieldName)
    {
        return await _characterDataAccess.RemoveFieldOnCharacter(character, fieldName);
    }

    public async Task<Option<CharacterError>> AddFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value)
    {
        return await _characterDataAccess.CreateFieldOnCharacter(character, fieldName, value);
    }
}