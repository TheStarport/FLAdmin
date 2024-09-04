namespace FlAdmin.Configs;

public class FlAdminConfig
{
    public MongoConfig Mongo { get; set; } = new MongoConfig();
    public AuthenticationConfig authentication { get; set; } = new AuthenticationConfig();
    
}