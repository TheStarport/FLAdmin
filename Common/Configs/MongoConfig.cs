namespace FlAdmin.Common.Configs;

public class MongoConfig
{
    public string ConnectionString { get; set; } = "mongodb://localhost";
    public string DatabaseName { get; set; } = "FLHook";
    public string AccountCollectionName { get; set; } = "accounts";
    public string CharacterCollectionName { get; set; } = "characters";

    public string FlAdminLogCollectionName { get; set; } = "logs";
}