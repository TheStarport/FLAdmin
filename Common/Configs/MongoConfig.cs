namespace FlAdmin.Configs;

public class MongoConfig
{
    public string ConnectionString { get; set; } = "mongodb://localhost";
    public string DatabaseName { get; set; } = "flhook";
    public string AccountCollectionName { get; set; } = "accounts";
    public string CharacterCollectionName { get; set; } = "characters";
}