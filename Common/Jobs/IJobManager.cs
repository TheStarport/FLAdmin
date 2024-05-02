namespace Common.Jobs;

public interface IJobManager
{
	public Task ExecuteTrigger(JobTrigger trigger, CancellationToken token);
}
