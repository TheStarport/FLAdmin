using System.Numerics;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface IFlHookService
{
    public Task<Option<FLAdminError>> PingFlHook();

    public Task<Either<FLAdminError, bool>> CharacterIsOnline(Either<string, ObjectId> characterName);
    public Task<Option<FLAdminError>> KickCharacter(Either<string, ObjectId> characterName);
    public Task<Option<FLAdminError>> KillCharacter(Either<string, ObjectId> characterName);

    public Task<Option<FLAdminError>> MessagePlayer(Either<string, ObjectId> characterName, string message);
    public Task<Option<FLAdminError>> MessageSystem(Either<string, int> system, string message);
    public Task<Option<FLAdminError>> MessageUniverse(string message);

    public Task<Option<FLAdminError>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName);

    public Task<Option<FLAdminError>> TeleportPlayerToSpot(Either<string, ObjectId> characterName,
        Either<string, int> system,
        Vector3? position);

    public Task<Either<FLAdminError, OnlinePlayerPayload>> GetOnlineCharacters();
}