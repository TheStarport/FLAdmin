using FlAdmin.Common.Configs;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using Flurl;
using Flurl.Http;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SharpDX;

namespace FlAdmin.Logic.Services;

public class FlHookService(FlAdminConfig config, ILogger<FlHookService> logger) : IFlHookService
{
    ILogger<FlHookService> _logger = logger;
    private readonly string _flHookUrl = config.FlHook.HttpUrl;


    public async Task<Either<FLAdminError, bool>> CharacterIsOnline(Either<string, ObjectId> characterName)
    {
        try
        {
            var isOnlineBytes = await characterName.Match(
                    Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                    Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray())))
                .AppendPathSegment("core/getonlinecharacter")
                .GetBytesAsync();
            
            var isOnlineBson = BsonSerializer.Deserialize<BsonDocument>(isOnlineBytes);
            if (!isOnlineBson.TryGetValue("isOnline", out var isOnline) || !isOnline.IsBoolean)
            {
                return FLAdminError.Unknown;
            }

            return isOnline.AsBoolean;
        }
        catch (BsonException ex)
        {
            _logger.LogError(ex, ex.Message);
            return FLAdminError.Unknown;
        }
        catch (FlurlHttpException e)
        {
            //TODO: Return Http error. 
            return FLAdminError.Unknown;
        }
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

    public Task<Option<FLAdminError>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> TeleportPlayerToSpot(Either<string, ObjectId> characterName,
        Either<string, int> system, float[]? position)
    {
        throw new NotImplementedException();
    }

    public Task<Either<FLAdminError, List<Character>>> GetOnlineCharacters()
    {
        throw new NotImplementedException();
    }
}