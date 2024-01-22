namespace Common.Models.Database;

using MongoDB.Bson.Serialization.Attributes;

public class Equipment
{
	[BsonElement("equipId")]
	public long EquipmentId { get; set; }
	[BsonElement("hardPoint")]
	public required string HardPoint { get; set; }
	[BsonElement("health")]
	public long Health { get; set; }
}
