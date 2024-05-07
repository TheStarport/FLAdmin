namespace Logic.Storage;

using Common.Configuration;
using Common.Jobs;
using Common.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

public class JobStorage : IJobStorage
{
	private readonly ILogger<JobStorage> _logger;
	private readonly FLAdminConfiguration _configuration;

	private IMongoCollection<Job> JobsCollection { get; }
	private IMongoCollection<JobGroup> JobsGroupCollection { get; }

	public JobStorage(ILogger<JobStorage> logger, FLAdminConfiguration configuration, IMongoManager mongo)
	{
		_logger = logger;
		_configuration = configuration;

		JobsCollection = mongo.GetCollection<Job>(_configuration.Mongo.JobCollection);
		JobsGroupCollection = mongo.GetCollection<JobGroup>(_configuration.Mongo.JobGroupCollection);
	}

	public async Task<List<JobGroup>> GetJobsByTrigger(JobTrigger trigger)
	{
		var aggregation = await JobsGroupCollection.Aggregate()
			.Match(Builders<JobGroup>.Filter.BitsAnySet(x => x.Triggers, (long)trigger))
			.Lookup(_configuration.Mongo.JobCollection, "jobs", "_id", "jobObjects")
			.ToListAsync();

		if (aggregation is null)
		{
			throw new BsonException("Failed to complete aggregation when grouping jobs by trigger.");
		}

		List<JobGroup> groups = [];
		groups.AddRange(aggregation.Select(doc => new JobGroup()
		{
			Name = doc["name"].AsString,
			Description = doc["description"].AsString,
			Created = doc["created"].AsBsonDateTime.ToLocalTime(),
			Updated = doc["updated"].AsBsonDateTime.ToLocalTime(),
			Triggers = (JobTrigger)doc["triggers"].AsInt32,
			CronTrigger = doc["cronTrigger"].AsString,
			JobsRefs = doc["jobs"].AsBsonArray.Select(x => x.AsObjectId).ToList(),
			Jobs = doc["jobObjects"].AsBsonArray.Select(x => BsonSerializer.Deserialize<Job>(x.AsBsonDocument)).ToList(),
		}));

		return groups;
	}

	public async Task<Job?> GetJobByName(string name) => await JobsCollection.Find(x => x.Name == name).FirstOrDefaultAsync();

	public async Task UpdateJob(Job jobUpdate) => await JobsCollection.FindOneAndReplaceAsync(x => x.Id == jobUpdate.Id, jobUpdate);
}
