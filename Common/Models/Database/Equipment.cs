using MongoDB.Bson.Serialization.Attributes;

namespace FlAdmin.Common.Models.Database;

public class Equipment
{
    [BsonElement("equipId")] public long EquipmentId { get; set; }

    [BsonElement("hardPoint")] public required string HardPoint { get; set; }

    [BsonElement("health")] public long Health { get; set; }
}