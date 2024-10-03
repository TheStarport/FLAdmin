namespace FlAdmin.Common.Configs;

public class FlAdminConfig
{
    public MongoConfig Mongo { get; set; } = new MongoConfig();
    public AuthenticationConfig Authentication { get; set; } = new AuthenticationConfig();
    public ServerConfig Server { get; set; } = new ServerConfig();
    public LoggingConfig Logging { get; set; } = new LoggingConfig();
}