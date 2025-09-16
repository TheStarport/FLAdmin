using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.Services;

public class ConfigService(FlAdminConfig config, ILogger<ConfigService> logger) : IConfigService
{
    private readonly string _freelancerDirectory = config.Server.FreelancerPath;

    public async Task<Either<FLAdminError, JsonDocument>> GetJsonConfig(string path, CancellationToken token)
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
            return FLAdminError.FileNotFound;
        }
        catch (JsonException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.FileNotValidJson;
        }
        catch (ArgumentException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.FileNotValidJson;
        }
    }

    public async Task<Either<FLAdminError, JsonDocument>> GetFlHookConfig(CancellationToken token)
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
            return FLAdminError.FileNotFound;
        }
        catch (JsonException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.FileNotValidJson;
        }
        catch (ArgumentException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.FileNotValidJson;
        }
    }

    public async Task<Either<FLAdminError, JsonDocument>> GetFlAdminConfig(CancellationToken token)
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
            return FLAdminError.FileNotFound;
        }
        catch (JsonException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.FileNotValidJson;
        }
        catch (ArgumentException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.FileNotValidJson;
        }
    }

    public Task<Option<FLAdminError>> SetJsonConfig(string path, JsonDocument json, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> SetFlHookConfig(JsonDocument json, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Option<FLAdminError>> SetFlAdminConfig(FlAdminConfig config, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Option<FLAdminError>> GenerateDefaultFlAdminConfig(CancellationToken token)
    {
        var newConfig = new FlAdminConfig();

        try
        {
            var writer = new StreamWriter("fladmin.json", false);
            await writer.WriteAsync(JsonSerializer.Serialize(newConfig));
            return Option<FLAdminError>.None;
        }
        //TODO: More specific saving error.
        catch (IOException e)
        {
            logger.LogError("{Message}", e.Message);
            return FLAdminError.Unknown;
        }
    }
}