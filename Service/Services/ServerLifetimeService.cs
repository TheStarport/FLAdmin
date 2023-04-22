namespace Service.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Managers;
using Common.State.ServerEvents;
using Fluxor;

public class ServerLifetimeService : IHostedService, IServerLifetime
{
	private Process? _flServer;
	private readonly ILogger<ServerLifetimeService> _logger;
	private readonly IConfiguration _config;
	private readonly IDispatcher _dispatcher;
	private readonly List<string> _consoleMessages = new();

	public ServerLifetimeService(ILogger<ServerLifetimeService> logger, IConfiguration configuration, IDispatcher dispatcher)
	{
		_logger = logger;
		_config = configuration;
		_dispatcher = dispatcher;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var path = Environment.ExpandEnvironmentVariables(_config.GetValue<string>("FreelancerPath"));
		var port = _config.GetValue<int>("FlPort");
		if (port == 0)
		{
			port = 2302;
		}

		ProcessStartInfo startInfo = new()
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			Arguments = $"-noconsole /p{port} /c",
			WorkingDirectory = Path.Combine(path, "EXE"),
			FileName = Path.Combine(path, "EXE", "FLServer.exe")
		};

		try
		{
			_flServer = Process.Start(startInfo);
			if (_flServer is null)
			{
				throw new InvalidOperationException("Unable to start FLServer");
			}

			_flServer.OutputDataReceived += (object _, DataReceivedEventArgs args) => AddLog(args?.Data ?? "");
			_flServer.BeginOutputReadLine();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to start process");
		}

		return Task.CompletedTask;
	}
	public Task StopAsync(CancellationToken cancellationToken)
	{
		if (_flServer is not null && !_flServer.HasExited)
		{
			_flServer.CancelOutputRead();
			_flServer.Kill();
			_flServer.Dispose();
			_flServer = null;
		}

		return Task.CompletedTask;
	}

	public void AddLog(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		_consoleMessages.Add(message);
		_dispatcher.Dispatch(new ConsoleMessageAction(message));
	}
	public void SendCommandToConsole(string command) => _flServer?.StandardInput.WriteLine(command);
	public IEnumerable<string> GetConsoleMessages(int page) => _consoleMessages
		.AsEnumerable()
		.Reverse()
		.Skip((page - 1) * 50)
		.Take(50);
	public int GetMessageCount() => _consoleMessages.Count;
}
