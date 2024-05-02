namespace Logic.Jobs;

using Common.Jobs;
using Quartz;
using System.Collections.Generic;
using Common.Storage;
using Quartz.Spi;

public class JobManager : IJobManager
{
	private readonly ISchedulerFactory _schedulerFactory;
	private readonly IJobStorage _jobStorage;
	private readonly IMongoManager _mongo;

	public JobManager(ISchedulerFactory schedulerFactory, IMongoManager mongo, IJobStorage jobStorage)
	{
		_schedulerFactory = schedulerFactory;
		_mongo = mongo;
		_jobStorage = jobStorage;

		// TODO: Register all jobs that have cron timers set
		// TODO: Load jobs from the redis database
	}

	public async Task ExecuteTrigger(JobTrigger trigger, CancellationToken token)
	{
		var db = _mongo.GetDatabase();
		var groups = await _jobStorage.GetJobGroupByTrigger(trigger);

		foreach (var group in groups)
		{
			foreach (var jobRef in group.Jobs)
			{

			}

			var jobType = job.Data.GetJobType();

			var jobBuilder = JobBuilder.Create(jobType.Item1)
				.WithIdentity(job.Name)
				.WithDescription(job.Description);

			// If job params were specified,
			if (jobType.Item2 is not null)
			{
				jobBuilder.SetJobData(jobType.Item2);
			}

			var triggerNow = TriggerBuilder.Create()
				.WithIdentity(job.Name)
				.WithSimpleSchedule()
				.StartNow()
				.Build();

			var scheduler = await _schedulerFactory.GetScheduler(token);
			_ = await scheduler.ScheduleJob(jobBuilder.Build(), triggerNow, token);
		}
	}
}
