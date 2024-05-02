namespace Common.Configuration;

public class MongoSettings
{
	public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
	public string Host { get; set; } = "localhost";
	public int Port { get; set; } = 27017;
	public string Username { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string AuthDatabase { get; set; } = string.Empty;
	public string PrimaryDatabaseName { get; set; } = "FLHook";
	public string AccountCollectionName { get; set; } = "accounts";
	public string JobCollection { get; set; } = "jobs";
}
