namespace Common.Messaging.Messages;
using MongoDB.Bson.Serialization.Attributes;

[Serializable]
[BsonIgnoreExtraElements]
public class ServerStats
{
	[BsonElement("memoryUsage")] public int MemoryUsage { get; set; }
	[BsonElement("serverLoad")] public int ServerLoad { get; set; }
	[BsonElement("players")] public PlayerInfo[] Players { get; set; } = Array.Empty<PlayerInfo>();
}

[Serializable]
[BsonIgnoreExtraElements]
public class PlayerInfo
{
	[BsonElement("ipAddress")] public string IpAddress { get; set; } = string.Empty;
	[BsonElement("playerName")] public string Name { get; set; } = string.Empty;
	[BsonElement("systemName")] public string SystemName { get; set; } = string.Empty;
	[BsonElement("systemNickname")] public string SystemNickname { get; set; } = string.Empty;
}
