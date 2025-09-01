using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.Services;

public class ConfigService(FlAdminConfig config, Logger<ConfigService> logger) : IConfigService
{
    private readonly string _freelancerDirectory = config.Server.FreelancerPath;

    public async Task<Either<FLAdminError, JsonDocument>> GetJsonConfig(string path)
    {
        try
        {
            var r = await File.ReadAllTextAsync(_freelancerDirectory + "/" + path);

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

    public async Task<Either<FLAdminError, JsonDocument>> GetFlHookConfig()
    {
        try
        {
            var r = await File.ReadAllTextAsync(_freelancerDirectory + "/" + "flhook.json");

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
}