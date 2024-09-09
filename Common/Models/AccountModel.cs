using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlAdmin.Common.Models;

public class AccountModel
{
    [BsonId] public required string Id { get; set; }

    [BsonElement("characters")] public List<ObjectId> Characters { get; set; } = [];
    [BsonElement("scheduledUnbanDate")] public DateTimeOffset? ScheduledUnbanDate { get; set; }
    [BsonElement("lastOnline")] public DateTimeOffset? LastOnline { get; set; }
    [BsonElement("gameRoles")] public List<string> GameRoles { get; set; } = new();
    [BsonElement("webRoles")] public List<string> WebRoles { get; set; } = new();
    [BsonElement("cash")] public long Cash { get; set; }
    
    [BsonExtraElements] public BsonDocument? Extra { get; set; }

    public bool IsGameAdmin => GameRoles.Count is not 0;
    public bool HasWebAccess => WebRoles.Count is not 0;
    public bool Banned => ScheduledUnbanDate is null;

}