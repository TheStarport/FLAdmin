namespace Common.Models.Database;

using MongoDB.Bson.Serialization.Attributes;

public class Costume
{
	[BsonElement("body")]
	public long Body { get; set; }
	[BsonElement("head")]
	public long Head { get; set; }
	[BsonElement("leftHand")]
	public long LeftHand { get; set; }
	[BsonElement("rightHand")]
	public long RightHand { get; set; }
}
