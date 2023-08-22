namespace Common.Jobs;

public class JobGroup
{
	public required string Name { get; set; }
	public required string Description { get; set; }
	public DateTime Created { get; set; }
	public DateTime Updated { get; set; }
	public List<JobTrigger> Triggers { get; set; } = new();
	public string? CronTrigger { get; set; }
	// TODO: Make it so only the manager can alter the jobs
	public List<Job> Jobs { get; set; } = new();
}
