namespace Common.Models.Database;

using MongoDB.Bson.Serialization.Attributes;

public class NpcVisit
{
	[BsonElement("npcId")]
	public long NpcId { get; set; }
	[BsonElement("baseId")]
	public long BaseId { get; set; }
	[BsonElement("interactionCount")]
	public int InteractionCount { get; set; }
	[BsonElement("missionStatus")]
	public int MissionStatus { get; set; }
}
