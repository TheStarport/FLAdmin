using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Models.Error;
using LanguageExt;

namespace FlAdmin.Common.Services;



public interface IConfigService
{
    /// <summary>
    /// Fetches a JSON file from the specified path relative to freelancer's base directory.
    /// </summary>
    /// <param name="path">Path to the json config file relative to Freelancer's base directory.</param>
    /// <returns>Either an error (usually file not found or file is not JSON, or the specified JSON</returns>
    public Task<Either<FLAdminError, JsonDocument>> GetJsonConfig(string path);
    
    /// <summary>
    /// Gets FLAdmin's config JSON
    /// </summary>
    /// <returns>Either an error (usually file not found or file is not JSON), or the specified JSON</returns>
    public Task<Either<FLAdminError, JsonDocument>> GetFlHookConfig();
    
    public Task<Either<FLAdminError, JsonDocument>> GetFlAdminConfig();
    
    public Task<Option<FLAdminError>> SetJsonConfig(string path, JsonDocument json);
    
    public Task<Option<FLAdminError>> SetFlHookConfig(JsonDocument json);
    
    public Task<Option<FLAdminError>> SetFlAdminConfig(FlAdminConfig config);
    
    /// <summary>
    /// Generates a default config for FLAdmin and saves it to file, see FLAdminConfig class for more info
    /// </summary>
    /// <returns>Error object if the operation fails, otherwise void if successul</returns>
    public Task<Option<FLAdminError>> GenerateDefaultFladminConfig();
}