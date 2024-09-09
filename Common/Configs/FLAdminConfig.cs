using FlAdmin.Common.Configs;

namespace FlAdmin.Configs;

public class FlAdminConfig
{
    public MongoConfig Mongo { get; set; } = new MongoConfig();
    public AuthenticationConfig Authentication { get; set; } = new AuthenticationConfig();
    public ServerConfig Server { get; set; } = new ServerConfig();
}