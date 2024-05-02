namespace Common.Jobs;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quartz;

public class Job
{
	[BsonId]
	[BsonElement("_id")]
	public ObjectId Id { get; set; }

	[BsonElement("name")]
	public required string Name { get; set; }
	[BsonElement("description")] public required string Description { get; set; }
	[BsonElement("jobData")] public required JobData Data { get; set; }
}

[BsonDiscriminator(RootClass = true)]
public abstract class JobData
{
	[BsonElement("jobType")] public required string JobType { get; set; }

	public abstract string GetDescription();
	public abstract (AlertType, string)? GetAlert();
	public abstract (Type, JobDataMap?) GetJobType();
}

public enum AlertType
{
	Info,
	Warning
}
