using System.Numerics;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Models.Payloads;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface IFlHookService
{
    public Task<Option<ErrorResult>> PingFlHook(CancellationToken token);

    public Task<Either<ErrorResult, bool>> CharacterIsOnline(Either<ObjectId, string> characterName,
        CancellationToken token);
    public Task<Option<ErrorResult>> KickCharacter(Either<string, ObjectId> characterName, CancellationToken token);
    public Task<Option<ErrorResult>> KillCharacter(Either<string, ObjectId> characterName, CancellationToken token);

    public Task<Option<ErrorResult>> MessagePlayer(Either<string, ObjectId> characterName, string message,
        CancellationToken token);
    public Task<Option<ErrorResult>> MessageSystem(Either<string, int> system, string message, CancellationToken token);
    public Task<Option<ErrorResult>> MessageUniverse(string message, CancellationToken token);

    public Task<Option<ErrorResult>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName, CancellationToken token);

    public Task<Option<ErrorResult>> TeleportPlayerToSpot(Either<string, ObjectId> characterName,
        Either<string, int> system,
        Vector3? position,
        CancellationToken token);

    public Task<Either<ErrorResult, OnlinePlayerPayload>> GetOnlineCharacters(CancellationToken token);
}