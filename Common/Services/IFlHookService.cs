using System.Numerics;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Models.Payloads;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface IFlHookService
{
    public Task<Option<FLAdminErrorCode>> PingFlHook(CancellationToken token);

    public Task<Either<FLAdminErrorCode, bool>> CharacterIsOnline(Either<ObjectId, string> characterName,
        CancellationToken token);
    public Task<Option<FLAdminErrorCode>> KickCharacter(Either<string, ObjectId> characterName, CancellationToken token);
    public Task<Option<FLAdminErrorCode>> KillCharacter(Either<string, ObjectId> characterName, CancellationToken token);

    public Task<Option<FLAdminErrorCode>> MessagePlayer(Either<string, ObjectId> characterName, string message,
        CancellationToken token);
    public Task<Option<FLAdminErrorCode>> MessageSystem(Either<string, int> system, string message, CancellationToken token);
    public Task<Option<FLAdminErrorCode>> MessageUniverse(string message, CancellationToken token);

    public Task<Option<FLAdminErrorCode>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName, CancellationToken token);

    public Task<Option<FLAdminErrorCode>> TeleportPlayerToSpot(Either<string, ObjectId> characterName,
        Either<string, int> system,
        Vector3? position,
        CancellationToken token);

    public Task<Either<FLAdminErrorCode, OnlinePlayerPayload>> GetOnlineCharacters(CancellationToken token);
}