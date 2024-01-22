namespace Common.Models.Database;

using Auth;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Account
{
	[BsonId]
	public required string Id { get; set; }

	[BsonElement("characters")] public List<Character> Characters { get; set; } = new();
	[BsonElement("banned")] public bool Banned { get; set; }
	[BsonElement("scheduledUnbanDate")] public DateTimeOffset? ScheduledUnbanDate { get; set; }
	[BsonElement("roles")] public List<string> Roles { get; set; } = new();
	[BsonElement("money")] public long Cash { get; set; }
	[BsonElement("username")] public string Username { get; set; } = string.Empty;
	[BsonElement("password")] public string PasswordHash { get; set; } = string.Empty;
	[BsonElement("hashedToken")] public string? HashedToken { get; set; }
	[BsonElement("salt")] public byte[]? Salt { get; set; }

	public bool IsAdmin => Roles.Contains(Role.Web.ToString());
}
