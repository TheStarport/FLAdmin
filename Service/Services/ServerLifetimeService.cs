namespace Service.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Managers;
using Common.State.ServerEvents;
using Fluxor;
using Serilog.Context;

public class ServerLifetimeService : BackgroundService, IServerLifetime
{
	private Process? _flServer;
	private readonly List<string> _consoleMessages = new();
	private bool _readyToStart;
	private readonly ILogger<ServerLifetimeService> _logger;
	private readonly FLAdminConfiguration _configuration;
	private readonly IDispatcher _dispatcher;

	public ServerLifetimeService(ILogger<ServerLifetimeService> logger, FLAdminConfiguration configuration, IDispatcher dispatcher)
	{
		_logger = logger;
		_configuration = configuration;
		_dispatcher = dispatcher;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			if (!_readyToStart || (_configuration.Server.AutoStartFLServer && string.IsNullOrWhiteSpace(_configuration.Server.FreelancerPath)))
			{
				// Await or we block the main thread from starting
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
				continue;
			}

			try
			{
				if ((_flServer is null || _flServer.HasExited) && !await StartProcess())
				{
					continue;
				}

				await _flServer!.WaitForExitAsync(stoppingToken);
			}
			finally
			{
				if (_flServer is not null && !_flServer.HasExited)
				{
					_flServer.CancelOutputRead();
					_flServer.Kill();
					_flServer.Dispose();
					_flServer = null;
				}
			}
		}
	}

	public void SendCommandToConsole(string command) => _flServer?.StandardInput.WriteLine(command);
	public void Terminate()
	{
		_ = (_flServer?.CloseMainWindow());
		_readyToStart = false;
	}

	public IEnumerable<string> GetConsoleMessages(int page) => _consoleMessages
		.AsEnumerable()
		.Reverse()
		.Skip((page - 1) * 50)
		.Take(50);

	public int GetMessageCount() => _consoleMessages.Count;

	private async Task<bool> StartProcess()
	{
		if (string.IsNullOrWhiteSpace(_configuration.Server.FreelancerPath))
		{
			return false;
		}

		var path = Environment.ExpandEnvironmentVariables(_configuration.Server.FreelancerPath);

		var exePath = Path.Combine(path, "EXE");
		var fileNamePath = Path.Combine(exePath, "FLServer.exe");

		if (_configuration.Server.CloseFLServerIfAlreadyOpen)
		{
			var activeProcesses = Process.GetProcessesByName(fileNamePath);
			if (activeProcesses.Length > 0)
			{
				foreach (var process in activeProcesses)
				{
					process.Close();
				}
			}
		}

		ProcessStartInfo startInfo = new()
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			Arguments = $"-noconsole /p{_configuration.Server.Port} /c {_configuration.Server.LaunchArgs}",
			WorkingDirectory = exePath,
			FileName = fileNamePath
		};

		try
		{
			_flServer = Process.Start(startInfo);
			if (_flServer is null)
			{
				throw new InvalidOperationException("Unable to start FLServer.exe");
			}

			_flServer.OutputDataReceived += (_, args) => AddLog(args?.Data ?? "");
			_flServer.BeginOutputReadLine();

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to start process");
			// Sleep as to not overwhelm with attempts
			await Task.Delay(TimeSpan.FromSeconds(20));
			return false;
		}
	}

	private void AddLog(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		using var prop = LogContext.PushProperty("FLHook", true);
		_logger.Log(LogLevel.Information, message);
		_consoleMessages.Add(message);
		_dispatcher.Dispatch(new ConsoleMessageAction(message));
	}

	public bool IsAlive() => _flServer is not null && !_flServer.HasExited;
	public bool ReadyToStart() => _readyToStart;

	public void Start() => _readyToStart = true;
}
