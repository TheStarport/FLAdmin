using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace FlAdmin.Logic.Services.Database;

public class CharacterService : ICharacterService
{
    private readonly IAccountDataAccess _accountDataAccess;
    private readonly ICharacterDataAccess _characterDataAccess;
    private readonly IFlHookService _flHookService;
    private readonly ILogger<CharacterService> _logger;
    private readonly IValidationService _validation;

    //TODO: Most of these functions need to implement checks to see if the character is online or not.

    public CharacterService(ICharacterDataAccess characterDataAccess, IAccountDataAccess accountDataAccess,
        IValidationService validator, IFlHookService flHookService,
        FlAdminConfig config, ILogger<CharacterService> logger)
    {
        _characterDataAccess = characterDataAccess;
        _accountDataAccess = accountDataAccess;
        _logger = logger;
        _validation = validator;
        _flHookService = flHookService;
    }


    public async Task<Either<FLAdminError, List<Character>>> GetCharactersOfAccount(string accountId,
        CancellationToken token)
    {
        var accountCheck = await _accountDataAccess.GetAccount(accountId, token);
        if (accountCheck.IsLeft)
            return accountCheck.Match
            (Left: err => err,
                Right: _ => FLAdminError.Unknown
            );


        var characters = await _characterDataAccess.GetCharactersByFilter(x => x.AccountId == accountId, token);

        if (characters.Count is 0) return FLAdminError.CharacterNotFound;

        return characters;
    }

    public async Task<Either<FLAdminError, Character>> GetCharacterByName(string name, CancellationToken token)
    {
        return await _characterDataAccess.GetCharacter(name, token);
    }

    public async Task<Option<FLAdminError>> AddCharacter(Character character, CancellationToken token)
    {
        if (_validation.ValidateCharacter(character) is false) return FLAdminError.InvalidCharacter;

        return await _characterDataAccess.CreateCharacters(token, character);
    }

    public async Task<Option<FLAdminError>> DeleteAllCharactersOnAccount(string accountId, CancellationToken token)
    {
        var characters = await _characterDataAccess.GetCharactersByFilter(x => x.AccountId == accountId, token);

        foreach (var c in characters)
        {
            var res = await _flHookService.CharacterIsOnline(c.CharacterName, token);
            var isOnline = res.Match(
                Left: err => true,
                Right: b => b is true);
            if (isOnline) return FLAdminError.CharacterIsLoggedIn;
        }

        if (characters.Count is 0) return FLAdminError.CharacterNotFound;

        var charNames = characters.Select(character => character.CharacterName).ToList();

        var result = await _characterDataAccess.DeleteCharacters(token, charNames.ToArray());
        if (result.IsSome) return result;

        List<ObjectId> cleanedCharacterList = new();
        var result2 =
            await _accountDataAccess.UpdateFieldOnAccount(accountId, "characters", cleanedCharacterList, token);

        return result2;
    }

    public async Task<Option<FLAdminError>> DeleteCharacter(Either<ObjectId, string> character, CancellationToken token)
    {
        var isOnlineRes = await _flHookService.CharacterIsOnline(character, token);
        var isOnline = isOnlineRes.Match(
            Left: err => true,
            Right: b => b is true);
        if (isOnline) return FLAdminError.CharacterIsLoggedIn;

        var charRes = await _characterDataAccess.GetCharacter(character, token);
        if (charRes.IsLeft)
            return charRes.Match(
                Left: err => err,
                //Right should never really be reached as we're checking it.
                Right: _ => FLAdminError.InvalidCharacter);

        var ch = charRes.Match<Character>(
            Left: _ => null!,
            Right: ch => ch);


        var accountRes = await _accountDataAccess.GetAccount(ch.AccountId, token);
        if (accountRes.IsLeft)
            return accountRes.Match(
                Left: err => err,
                Right: err => FLAdminError.Unknown);

        var account = accountRes.Match<Account>(
            Left: _ => null!,
            Right: x => x);
        account.Characters.Remove(ch.Id);

        var res = await _characterDataAccess.DeleteCharacters(token, ch.CharacterName);
        if (res.IsSome) return res;

        var res2 = await _accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
        return res2;
    }

    public async Task<Option<FLAdminError>> MoveCharacter(Either<ObjectId, string> character, string newAccountId,
        CancellationToken token)
    {
        var isOnlineRes = await _flHookService.CharacterIsOnline(character, token);
        var isOnline = isOnlineRes.Match(
            Left: err => true,
            Right: b => b is true);
        if (isOnline) return FLAdminError.CharacterIsLoggedIn;

        var charRes = await _characterDataAccess.GetCharacter(character, token);
        if (charRes.IsLeft)
            return charRes.Match(
                Left: err => err, Right
                //Right should never really be reached as we're checking it.
                : _ => FLAdminError.InvalidCharacter);

        var ch = charRes.Match<Character>(
            Left: _ => null!,
            Right: ch => ch);
        var oldAccountRes = await _accountDataAccess.GetAccount(ch.AccountId, token);
        if (oldAccountRes.IsLeft)
            return oldAccountRes.Match(
                Left: err => err,
                Right: _ => FLAdminError.Unknown);

        var oldAccount = oldAccountRes.Match<Account>(
            Left: _ => null!,
            Right: x => x);

        var newAccountRes = await _accountDataAccess.GetAccount(newAccountId, token);
        if (newAccountRes.IsLeft)
            return newAccountRes.Match(
                Left: err => err,
                Right: _ => FLAdminError.Unknown);

        var newAccount = newAccountRes.Match<Account>(
            Left: _ => null!,
            Right: x => x);


        oldAccount.Characters.Remove(ch.Id);
        newAccount.Characters.Add(ch.Id);

        var result1 = await _characterDataAccess.UpdateFieldOnCharacter(character, "accountId", newAccountId, token);
        if (result1.IsSome) return result1;

        var result2 =
            await _accountDataAccess.UpdateFieldOnAccount(oldAccount.Id, "characters", oldAccount.Characters, token);
        if (result2.IsSome) return result2;

        var result3 =
            await _accountDataAccess.UpdateFieldOnAccount(newAccount.Id, "characters", newAccount.Characters, token);
        if (result3.IsSome) return result3;

        return new Option<FLAdminError>();
    }

    public async Task<Option<FLAdminError>> UpdateCharacter(Character character, CancellationToken token)
    {
        var isOnlineRes = await _flHookService.CharacterIsOnline(character.CharacterName, token);
        var isOnline = isOnlineRes.Match(
            Left: err => true,
            Right: b => b is true);
        if (isOnline) return FLAdminError.CharacterIsLoggedIn;

        if (_validation.ValidateCharacter(character) is false) return FLAdminError.InvalidCharacter;

        return await _characterDataAccess.UpdateCharacter(character.ToBsonDocument(), token);
    }

    public async Task<Option<FLAdminError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value, CancellationToken token)
    {
        var isOnlineRes = await _flHookService.CharacterIsOnline(character, token);
        var isOnline = isOnlineRes.Match(
            Left: err => true,
            Right: b => b is true);
        if (isOnline) return FLAdminError.CharacterIsLoggedIn;

        return await _characterDataAccess.UpdateFieldOnCharacter(character, fieldName, value, token);
    }

    public async Task<Option<FLAdminError>> RemoveFieldOnCharacter(Either<ObjectId, string> character,
        string fieldName, CancellationToken token)
    {
        var isOnlineRes = await _flHookService.CharacterIsOnline(character, token);
        var isOnline = isOnlineRes.Match(
            Left: err => true,
            Right: b => b is true);
        if (isOnline) return FLAdminError.CharacterIsLoggedIn;

        return await _characterDataAccess.RemoveFieldOnCharacter(character, fieldName, token);
    }

    public async Task<Option<FLAdminError>> AddFieldOnCharacter<T>(Either<ObjectId, string> character,
        string fieldName,
        T value, CancellationToken token)
    {
        return await _characterDataAccess.CreateFieldOnCharacter(character, fieldName, value, token);
    }

    public async Task<Option<FLAdminError>> RenameCharacter(string oldName, string newName, CancellationToken token)
    {
        var isOnlineRes = await _flHookService.CharacterIsOnline(oldName, token);
        var isOnline = isOnlineRes.Match(
            Left: err => true,
            Right: b => b is true);
        if (isOnline) return FLAdminError.CharacterIsLoggedIn;

        //First check if new name is available.
        var newNameAvailableCheck = await _characterDataAccess.GetCharacter(newName, token);

        if (newNameAvailableCheck.IsRight) return FLAdminError.CharacterNameIsTaken;

        var characterRes = await _characterDataAccess.GetCharacter(oldName, token);
        if (characterRes.IsLeft)
            return characterRes.Match(
                Left: err => err,
                Right: _ => FLAdminError.Unknown
            );

        var character = characterRes.Match<Character>(
            Left: _ => null!,
            Right: ch => ch
        );

        character.CharacterName = newName;

        return await _characterDataAccess.UpdateCharacter(character.ToBsonDocument(), token);
    }

    public async Task<Either<FLAdminError, List<CharacterSummary>>> GetCharacterSummaries(BsonDocument filter, int page,
        int pageSize, CancellationToken token)
    {
        var characters = await _characterDataAccess.GetCharactersByFilter(filter, token, page, pageSize);

        if (characters.IsNullOrEmpty()) return FLAdminError.CharacterNotFound;

        var summaries = new List<CharacterSummary>(characters.Select(character => new CharacterSummary
        {
            Id = character.Id,
            Name = character.CharacterName,
            Money = character.Money,
            AccountId = character.AccountId,
            Base = character.CurrentBase,
            Rep = character.RepGroup ?? ""
        }));

        return summaries;
    }
}