namespace Common.Models.Database;

using MongoDB.Bson.Serialization.Attributes;

public class Cargo
{
	[BsonElement("cargoId")]
	public long CargoId { get; set; }
	[BsonElement("amount")]
	public long Amount { get; set; }
	[BsonElement("health")]
	public long Health { get; set; }
	[BsonElement("isMissionCargo")]
	public bool IsMissionCargo { get; set; }
}
