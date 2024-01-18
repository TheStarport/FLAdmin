namespace Logic.Jobs;

using Quartz;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

public class RunShellJob : IJob
{
	public const string Id = nameof(RunShellJob);

	public string Command { get; set; }
	public RunShellJob(string command) => Command = command;

	public Task Execute(IJobExecutionContext context)
	{
		using var pipeline = RunspaceFactory.CreateRunspace().CreatePipeline();

		pipeline.Commands.AddScript(Command);
		pipeline.Runspace.Open();

		// TODO: handle error
		var output = pipeline.Invoke();

		foreach (var result in output)
		{
			// TODO: Process result
			Console.WriteLine("Processed!");
		}

		return Task.CompletedTask;
	}
}
