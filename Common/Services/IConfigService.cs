using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using LanguageExt;

namespace FlAdmin.Common.Services;

public interface IConfigService
{
    /// <summary>
    ///     Fetches a JSON file from the specified path relative to freelancer's base directory.
    /// </summary>
    /// <param name="path">Path to the json config file relative to Freelancer's base directory.</param>
    /// <param name="token"></param>
    /// <returns>Either an error (usually file not found or file is not JSON, or the specified JSON</returns>
    public Task<Either<ErrorResult, JsonDocument>> GetJsonConfig(string path, CancellationToken token);

    /// <summary>
    ///     Gets FLAdmin's config JSON
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Either an error (usually file not found or file is not JSON), or the specified JSON</returns>
    public Task<Either<ErrorResult, JsonDocument>> GetFlHookConfig(CancellationToken token);


    /// <summary>
    /// Gets FLAdmin's Config
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Either an error (usually file not found or file is not JSON), or the specified JSON</returns>
    public Task<Either<ErrorResult, JsonDocument>> GetFlAdminConfig(CancellationToken token);

    /// <summary>
    /// Updates a specified JSON by replacing it with the provided json document
    /// </summary>
    /// <param name="path">path of the config file to update</param>
    /// <param name="json">the new updated JSON</param>
    /// <param name="token"></param>
    /// <returns>Either an error or null representing success.</returns>
    public Task<Option<ErrorResult>> SetJsonConfig(string path, JsonDocument json, CancellationToken token);

    /// <summary>
    /// Updates FLHook's json config
    /// </summary>
    /// <param name="json">the new updated JSON</param>
    /// <param name="token"></param>
    /// <returns>Either an error or null representing success.</returns>
    public Task<Option<ErrorResult>> SetFlHookConfig(JsonDocument json, CancellationToken token);

    /// <summary>
    /// Updates FLAdmin's config by replacing it with the provided document
    /// </summary>
    /// <param name="config">the new updated json</param>
    /// <param name="token"></param>
    /// <returns>Error object if the operation fails, otherwise void if successful</returns>
    public Task<Option<ErrorResult>> SetFlAdminConfig(FlAdminConfig config, CancellationToken token);

    /// <summary>
    ///     Generates a default config for FLAdmin and saves it to file, see FLAdminConfig class for more info
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Error object if the operation fails, otherwise void if successful</returns>
    public Task<Option<ErrorResult>> GenerateDefaultFlAdminConfig(CancellationToken token);
}