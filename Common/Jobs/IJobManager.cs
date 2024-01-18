namespace Common.Jobs;

public interface IJobManager
{
	public IReadOnlyCollection<JobGroup> GetGroups();
	public JobGroup? GetGroup(string groupName);
	public void AddJob(JobGroup group, Job job);
	public void RemoveJob(JobGroup group, Job job);
	public void RemoveJobByName(JobGroup group, string name);
	public Task ExecuteTrigger(JobTrigger trigger, CancellationToken token);
}
