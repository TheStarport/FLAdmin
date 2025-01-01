using System.Net;
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
    private readonly string _flHookUrl = config.FlHook.HttpUrl;
    FreelancerData _freelancerData = fldata.GetFreelancerData()!;


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
            await characterName.Match(
                    Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                    Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray())))
                .AppendPathSegment(FlHookApiRoutes.KickCharacter).PatchAsync();

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
            await characterName.Match(
                    Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                    Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray())))
                .AppendPathSegment(FlHookApiRoutes.KillCharacter).PatchAsync();

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
            await characterName.Match(
                    Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                    Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray())))
                .AppendQueryParam("message", message)
                .AppendPathSegment(FlHookApiRoutes.MessagePlayer).PatchAsync();

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
            //We can't async the whole function as FLHash is not awaitable
            var str = system.Match(
                Left: name => _flHookUrl.AppendQueryParam("system", FLHash.CreateID(name)),
                Right: id => _flHookUrl.AppendQueryParam("system", id)
                    .AppendQueryParam("message", message)
                    .AppendPathSegment(FlHookApiRoutes.MessageUniverse));
            await str.PatchAsync();

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
            await _flHookUrl.AppendQueryParam("message", message)
                .AppendPathSegment(FlHookApiRoutes.MessageSystem).PatchAsync();

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
            var str = baseName.Match(
                    Left: name => _flHookUrl.AppendQueryParam("base", FLHash.CreateID(name)),
                    Right: id => _flHookUrl.AppendQueryParam("base", id)
                        .AppendQueryParam(characterName.Match(
                            Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                            Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray()))))
                        .AppendPathSegment(FlHookApiRoutes.BeamPlayer))
                ;
            await str.PatchAsync();

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
        Either<string, int> system, float[]? position)
    {
        try
        {
            var str = system.Match(
                    Left: name => _flHookUrl.AppendQueryParam("system", FLHash.CreateID(name)),
                    Right: id => _flHookUrl.AppendQueryParam("system", id)
                        .AppendQueryParam(characterName.Match(
                            Left: str => _flHookUrl.AppendQueryParam("characterName", str),
                            Right: obj => _flHookUrl.AppendQueryParam("id", Convert.ToBase64String(obj.ToByteArray()))))
                        .AppendQueryParam("position", position ?? [0, 0, 0])
                        .AppendPathSegment(FlHookApiRoutes.TeleportPlayer))
                ;
            await str.PatchAsync();

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

    public async Task<Either<FLAdminError, List<Character>>> GetOnlineCharacters()
    {
        try
        {
            var onlineCharactersBytes = await
                _flHookUrl.AppendPathSegment(FlHookApiRoutes.GetOnlineCharacters).GetBytesAsync();

            return BsonSerializer.Deserialize<List<Character>>(onlineCharactersBytes);
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