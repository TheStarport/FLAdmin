namespace FlAdmin.Common.Configs;

public class FlAdminConfig
{
    public MongoConfig Mongo { get; set; } = new();
    public AuthenticationConfig Authentication { get; set; } = new();
    public ServerConfig Server { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();

    public FlHookConfig FlHook { get; set; } = new();

    public string SuperAdminName { get; set; } = "SuperAdmin";

    public int MaxCharactersPerAccount { get; set; } = 5;
}