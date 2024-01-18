namespace Logic.Jobs;

using Common.Jobs;
using Quartz;
using System.Collections.Generic;

public class JobManager : IJobManager
{
	private readonly ISchedulerFactory _schedulerFactory;
	private HashSet<JobGroup> _jobGroups = new();

	private static IReadOnlyDictionary<string, Type> JobTypes => new Dictionary<string, Type>()
	{
		{ RunShellJob.Id, typeof(RunShellJob) },
	};

	public JobManager(ISchedulerFactory schedulerFactory)
	{
		_schedulerFactory = schedulerFactory;

		// TODO: Register all jobs that have cron timers set
		// TODO: Load jobs from the redis database
	}

	public void AddJob(JobGroup group, Job job) => _jobGroups.Add(group);

	public async Task ExecuteTrigger(JobTrigger trigger, CancellationToken token)
	{
		foreach (var group in _jobGroups)
		{
			if (!group.Triggers.Contains(trigger))
			{
				continue;
			}

			foreach (var job in group.Jobs)
			{
				if (!JobTypes.ContainsKey(job.JobId))
				{
					// TODO: Log invalid job with a warning
					continue;
				}

				// TODO: Handle job parameters

				var builtJob = JobBuilder.Create(JobTypes[job.JobId])
					.WithIdentity(job.Name, group.Name)
					.WithDescription(job.Description)
					.Build();

				var triggerNow = TriggerBuilder.Create()
					.WithIdentity(job.Name, group.Name)
					.WithSimpleSchedule()
					.StartNow()
					.Build();

				var scheduler = await _schedulerFactory.GetScheduler(token);
				_ = await scheduler.ScheduleJob(builtJob, triggerNow, token);
			}
		}
	}

	public JobGroup? GetGroup(string groupName) => _jobGroups.FirstOrDefault(x => x.Name == groupName);
	public IReadOnlyCollection<JobGroup> GetGroups() => _jobGroups;
	public void RemoveJob(JobGroup group, Job job) => group.Jobs.Remove(job);
	public void RemoveJobByName(JobGroup group, string name)
	{
		if (group.Jobs.Count == 0)
		{
			return;
		}

		var index = group.Jobs.FindIndex(x => x.Name == name);
		if (index >= 0)
		{
			group.Jobs.RemoveAt(index);
		}
	}
}
