namespace Common.Jobs.JobTypes;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Logic.Jobs;
using MongoDB.Bson.Serialization.Attributes;
using Quartz;

public class ShellJob : JobData
{
	[BsonElement("command")]
	public string? Command { get; set; }

	public override string GetDescription() => "Executes a predefined powershell script.";
	public override (AlertType, string)? GetAlert()
		=> (AlertType.Warning, "Executing scripts is very dangerous and should be treated as a major "+
		                       "security vulnerability if left unsecured.");

	public override (Type, JobDataMap?) GetJobType()
	{
		JobDataMap data = new() { { "command", Command ?? "" } };

		return (typeof(RunShellJob), data);
	}
}

public class RunShellJob : IJob
{
	public Task Execute(IJobExecutionContext context)
	{
		var command = context.JobDetail.JobDataMap.GetString("command") ?? "";
		if (command.Length is 0)
		{
			return Task.CompletedTask;
		}

		using var rs = RunspaceFactory.CreateRunspace();
		rs.Open();

		using var pipeline = rs.CreatePipeline();
		pipeline.Commands.AddScript(command);

		// Execute Script
		try
		{
			_ = pipeline.Invoke();
		}
		catch (Exception ex)
		{
			// TODO: Handle error
		}

		rs.Close();
		return Task.CompletedTask;
	}
}
