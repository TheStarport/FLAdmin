using System.Numerics;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Models.Payloads;
using FlAdmin.Common.Services;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Tests.DataAccessMocks;

public class FLHookServiceMock : IFlHookService
{
    public Task<Option<FLAdminError>> PingFlHook(CancellationToken token)
    {
        throw new NotImplementedException();
    }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<Either<FLAdminError, bool>> CharacterIsOnline(Either<ObjectId, string> characterName,
        CancellationToken token)
    {
        return false;
    }

    public Task<Option<FLAdminError>> KickCharacter(Either<string, ObjectId> characterName, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> KillCharacter(Either<string, ObjectId> characterName, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> MessagePlayer(Either<string, ObjectId> characterName, string message,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> MessageSystem(Either<string, int> system, string message, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> MessageUniverse(string message, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> TeleportPlayerToSpot(Either<string, ObjectId> characterName, Either<string, int> system, Vector3? position, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    

    public Task<Either<FLAdminError, OnlinePlayerPayload>> GetOnlineCharacters(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}