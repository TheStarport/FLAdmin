using System.Numerics;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Models.Payloads;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface IFlHookService
{
    public Task<Option<FLAdminError>> PingFlHook(CancellationToken token);

    public Task<Either<FLAdminError, bool>> CharacterIsOnline(Either<ObjectId, string> characterName,
        CancellationToken token);
    public Task<Option<FLAdminError>> KickCharacter(Either<string, ObjectId> characterName, CancellationToken token);
    public Task<Option<FLAdminError>> KillCharacter(Either<string, ObjectId> characterName, CancellationToken token);

    public Task<Option<FLAdminError>> MessagePlayer(Either<string, ObjectId> characterName, string message,
        CancellationToken token);
    public Task<Option<FLAdminError>> MessageSystem(Either<string, int> system, string message, CancellationToken token);
    public Task<Option<FLAdminError>> MessageUniverse(string message, CancellationToken token);

    public Task<Option<FLAdminError>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName, CancellationToken token);

    public Task<Option<FLAdminError>> TeleportPlayerToSpot(Either<string, ObjectId> characterName,
        Either<string, int> system,
        Vector3? position,
        CancellationToken token);

    public Task<Either<FLAdminError, OnlinePlayerPayload>> GetOnlineCharacters(CancellationToken token);
}