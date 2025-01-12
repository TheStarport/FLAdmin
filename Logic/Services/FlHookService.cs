using System.Net;
using System.Numerics;
using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using Flurl;
using Flurl.Http;
using LanguageExt;
using LibreLancer;
using LibreLancer.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace FlAdmin.Logic.Services;

public class FlHookService(FlAdminConfig config, ILogger<FlHookService> logger, IFreelancerDataProvider fldata)
    : IFlHookService
{
    ILogger<FlHookService> _logger = logger;
    private readonly string _flHookUrl = config.FlHook.HttpUrl + config.Server.Port;
    FreelancerData _freelancerData = fldata.GetFreelancerData()!;


    public async Task<Option<FLAdminError>> PingFlHook()
    {
        try
        {
            var ping = 
                 await _flHookUrl.AppendPathSegment(FlHookApiRoutes.Ping)
                .GetAsync();

            if (ping.StatusCode is (int) HttpStatusCode.OK)
            {
                return new Option<FLAdminError>();
            }

            return FLAdminError.FlHookFailedToRespond;
        }
        catch (BsonException ex)
        {
            _logger.LogError(ex, ex.Message);
            return FLAdminError.FlHookHttpError;
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Either<FLAdminError, bool>> CharacterIsOnline(Either<string, ObjectId> characterName)
    {
        try
        {
            var isOnlineBytes = await characterName.Match(
                    Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                    Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray())))
                .AppendPathSegment(FlHookApiRoutes.CharacterIsOnline)
                .GetBytesAsync();

            var isOnlineBson = BsonSerializer.Deserialize<BsonDocument>(isOnlineBytes);
            if (!isOnlineBson.TryGetValue("isOnline", out var isOnline) || !isOnline.IsBoolean)
            {
                return FLAdminError.FlHookHttpError;
            }

            return isOnline.AsBoolean;
        }
        catch (BsonException ex)
        {
            _logger.LogError(ex, ex.Message);
            return FLAdminError.FlHookHttpError;
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> KickCharacter(Either<string, ObjectId> characterName)
    {
        try
        {
            var request = new BsonDocument();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            characterName.Match(
                Left: str => request.Set("characterName", str),
                Right: id => request.Set("id", Convert.ToBase64String(id.ToByteArray()))
            );

            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.KickCharacter);
            var content = new ByteArrayContent(request.ToBson());
            await str.PatchAsync(content);

            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> KillCharacter(Either<string, ObjectId> characterName)
    {
        try
        {
            var request = new BsonDocument();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            characterName.Match(
                Left: str => request.Set("characterName", str),
                Right: id => request.Set("id", Convert.ToBase64String(id.ToByteArray()))
            );

            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.KillCharacter);
            var content = new ByteArrayContent(request.ToBson());
            await str.PatchAsync(content);

            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> MessagePlayer(Either<string, ObjectId> characterName, string message)
    {
        try
        {
            var request = new BsonDocument();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            characterName.Match(
                Left: str => request.Set("characterName", str),
                Right: id => request.Set("id", Convert.ToBase64String(id.ToByteArray()))
            );
            request.Set("message", message);

            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.MessagePlayer);
            var content = new ByteArrayContent(request.ToBson());
            await str.PatchAsync(content);

            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> MessageSystem(Either<string, int> system, string message)
    {
        try
        {
            var request = new BsonDocument();

            var sysId = system.Match(
                Left: name => unchecked((int) FLHash.CreateID(name)),
                Right: id => id
            );
            request.Set("system", sysId);
            request.Set("message", message);

            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.MessageSystem);
            var content = new ByteArrayContent(request.ToBson());
            await str.PatchAsync(content);

            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> MessageUniverse(string message)
    {
        try
        {
            var request = new BsonDocument();

            request.Set("message", message);

            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.MessageUniverse);
            var content = new ByteArrayContent(request.ToBson());
            await str.PatchAsync(content);

            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> BeamPlayerToBase(Either<string, ObjectId> characterName,
        Either<string, int> baseName)
    {
        try
        {
            var request = new BsonDocument();

            var baseId = baseName.Match(
                Left: name => unchecked((int) FLHash.CreateID(name)),
                Right: id => id
            );
            request.Set("base", baseId);
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            characterName.Match(
                Left: str => request.Set("characterName", str),
                Right: id => request.Set("id", Convert.ToBase64String(id.ToByteArray()))
            );
            
            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.BeamPlayer);
            var content = new ByteArrayContent(request.ToBson());
            await str.PatchAsync(content);
            
            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Option<FLAdminError>> TeleportPlayerToSpot(Either<string, ObjectId> characterName,
        Either<string, int> system, Vector3? position)
    {
        try
        {
            var request = new BsonDocument();

            var sysId = system.Match(
                Left: name => unchecked((int) FLHash.CreateID(name)),
                Right: id => id
            );
            request.Set("system", sysId);
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            characterName.Match(
                Left: str => request.Set("characterName", str),
                Right: id => request.Set("id", Convert.ToBase64String(id.ToByteArray()))
            );
            if (position.HasValue)
            {
                request.Set("position", new BsonArray() {position.Value.X, position.Value.Y, position.Value.Z});
            }

            var content = new ByteArrayContent(request.ToBson());
            var str = _flHookUrl.AppendPathSegment(FlHookApiRoutes.TeleportPlayer);
            await str.PatchAsync(content);

            return new Option<FLAdminError>();
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }

    public async Task<Either<FLAdminError, OnlinePlayerPayload>> GetOnlineCharacters()
    {
        try
        {
            var onlineCharactersBytes = await
                _flHookUrl.AppendPathSegment(FlHookApiRoutes.GetOnlinePlayers).GetBytesAsync();

            return BsonSerializer.Deserialize<OnlinePlayerPayload>(onlineCharactersBytes);
        }
        catch (BsonException ex)
        {
            _logger.LogError(ex, ex.Message);
            return FLAdminError.FlHookHttpError;
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode is (int) HttpStatusCode.RequestTimeout or (int) HttpStatusCode.GatewayTimeout)
            {
                return FLAdminError.FLHookRequestTimeout;
            }

            return FLAdminError.FlHookHttpError;
        }
    }
}