using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.Services;

public class ConfigService(FlAdminConfig config, ILogger<ConfigService> logger) : IConfigService
{
    private readonly string _freelancerDirectory = config.Server.FreelancerPath;


    public async Task<Either<ErrorResult, JsonDocument>> GetJsonConfig(string path, CancellationToken token)
    {
        try
        {
            var r = await File.ReadAllTextAsync(_freelancerDirectory + "/" + path, token);

            return JsonDocument.Parse(r);
        }

        //TODO: More metadata on the exception.
        catch (IOException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotFound, $"file not found at path {path}");
        }
        catch (JsonException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotValidJson);
        }
        catch (ArgumentException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotValidJson);
        }
    }

    public async Task<Either<ErrorResult, JsonDocument>> GetFlHookConfig(CancellationToken token)
    {
        try
        {
            var r = await File.ReadAllTextAsync(_freelancerDirectory + "/" + "flhook.json", token);

            return JsonDocument.Parse(r);
        }
        //TODO: More metadata on the exception.
        catch (IOException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotFound, $"flhook config not found");
        }
        catch (JsonException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotValidJson);
        }
        catch (ArgumentException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotValidJson);
        }
    }

    public async Task<Either<ErrorResult, JsonDocument>> GetFlAdminConfig(CancellationToken token)
    {
        try
        {
            var r = await File.ReadAllTextAsync("fladmin.json", token);

            return JsonDocument.Parse(r);
        }
        //TODO: More metadata on the exception.
        catch (IOException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotFound, $"FLAdmin config does not exist.");
        }
        catch (JsonException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotValidJson);
        }
        catch (ArgumentException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.FileNotValidJson);
        }
    }

    public Task<Option<ErrorResult>> SetJsonConfig(string path, JsonDocument json, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<ErrorResult>> SetFlHookConfig(JsonDocument json, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<ErrorResult>> SetFlAdminConfig(FlAdminConfig config, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Option<ErrorResult>> GenerateDefaultFlAdminConfig(CancellationToken token)
    {
        var newConfig = new FlAdminConfig();

        try
        {
            var writer = new StreamWriter("fladmin.json", false);
            await writer.WriteAsync(JsonSerializer.Serialize(newConfig));
            return Option<ErrorResult>.None;
        }
        //TODO: More specific saving error.
        catch (IOException e)
        {
            logger.LogError("{Message}", e.Message);
            return new ErrorResult(FLAdminErrorCode.Unknown);
        }
    }
}