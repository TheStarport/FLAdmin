using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using Flurl;
using Flurl.Http;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Logic.Services;

public class FlHookService : IFlHookService
{
 
    public Task<Either<FLAdminError, bool>> CharacterIsOnline(Either<string, ObjectId> characterName)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> KickCharacter(Either<string, ObjectId> characterName)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> KillCharacter(Either<string, ObjectId> characterName)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> MessagePlayer(Either<string, ObjectId> characterName, string message)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> MessageSystem(Either<string, int> system, string message)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> MessageUniverse(string message)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> BeamPlayerToBase(Either<string, ObjectId> characterName, Either<string, int> baseName)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> TeleportPlayerToSpot(Either<string, ObjectId> characterName, Either<string, int> system, float[]? position)
    {
        throw new NotImplementedException();
    }

    public Task<Either<FLAdminError, List<Character>>> GetOnlineCharacters()
    {
        throw new NotImplementedException();
    }
}