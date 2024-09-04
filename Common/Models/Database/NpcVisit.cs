using MongoDB.Bson.Serialization.Attributes;

namespace FlAdmin.Common.Models.Database;

public class NpcVisit
{
    [BsonElement("npcId")] public long NpcId { get; set; }

    [BsonElement("baseId")] public long BaseId { get; set; }

    [BsonElement("interactionCount")] public int InteractionCount { get; set; }

    [BsonElement("missionStatus")] public int MissionStatus { get; set; }
}