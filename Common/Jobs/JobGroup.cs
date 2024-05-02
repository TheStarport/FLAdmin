namespace Common.Jobs;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public class JobGroup
{
	[BsonId]
	[BsonElement("_id")]
	public ObjectId Id { get; set; }

	[BsonElement("name")] public required string Name { get; set; }
	[BsonElement("description")] public required string Description { get; set; }
	[BsonElement("created")] public DateTime Created { get; set; }
	[BsonElement("updated")] public DateTime Updated { get; set; }

	[BsonElement("triggers")] public JobTrigger Triggers { get; set; } = JobTrigger.Manual;
	[BsonElement("cronTrigger")] public string? CronTrigger { get; set; }
	// TODO: Make it so only the manager can alter the jobs

	[BsonElement("jobs")] public List<ObjectId> JobsRefs { get; set; } = new();
	[BsonIgnore] public List<Job> Jobs { get; set; } = new();
}
