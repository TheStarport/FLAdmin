namespace Logic.Storage;

using Common.Configuration;
using Common.Jobs;
using Common.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class JobStorage : IJobStorage
{
	private readonly ILogger<JobStorage> _logger;
	private readonly FLAdminConfiguration _configuration;
	private readonly IMongoManager _mongo;

	private IMongoCollection<Job>? JobsCollection { get; set; }
	private IMongoCollection<JobGroup>? JobsGroupCollection { get; set; }

	public JobStorage(Logger<JobStorage> logger, FLAdminConfiguration configuration, IMongoManager mongo)
	{
		_logger = logger;
		_configuration = configuration;
		_mongo = mongo;
	}

	public async Task<List<JobGroup>> GetJobGroupByTrigger(JobTrigger trigger)
	{
		await EnsureCollection();

		var aggregation = JobsGroupCollection.Aggregate()
			.Match(Builders<JobGroup>.Filter.BitsAnySet(x => x.Triggers, (long)trigger))
			.Unwind(x => x.JobsRefs)
			.Lookup(_configuration.Mongo.JobCollection, localField: "_id", foreignField: )
			.Group()

		return aggregation;
	}

	public async Task<Job?> GetJobByName(string name)
	{
		await EnsureCollection();

		return await JobsCollection.Find(x => x.Name == name).FirstOrDefaultAsync();
	}

	public async Task UpdateJob(Job jobUpdate)
	{
		await EnsureCollection();

		await JobsCollection.FindOneAndReplaceAsync(x => x.Id == jobUpdate.Id, jobUpdate);
	}

	private async Task EnsureCollection()
	{
		JobsCollection ??= await _mongo.GetCollectionAsync<Job>(_configuration.Mongo.JobCollection);
		JobsGroupCollection ??= await _mongo.GetCollectionAsync<JobGroup>(_configuration.Mongo.JobCollection);
	}
}
