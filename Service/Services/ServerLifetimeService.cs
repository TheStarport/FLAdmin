namespace Service.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Managers;
using Common.State.ServerEvents;
using Fluxor;

public class ServerLifetimeService : BackgroundService, IServerLifetime
{
	private Process? _flServer;
	private readonly ILogger<ServerLifetimeService> _logger;
	private readonly FLAdminConfiguration _config;
	private readonly IDispatcher _dispatcher;
	private readonly List<string> _consoleMessages = new();
	private bool _readyToStart = false;

	public ServerLifetimeService(ILogger<ServerLifetimeService> logger, FLAdminConfiguration configuration, IDispatcher dispatcher)
	{
		_logger = logger;
		_config = configuration;
		_dispatcher = dispatcher;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			if (!_readyToStart && !_config.Server.AutoStartFLServer)
			{
				// Await or we block the main thread from starting
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
				continue;
			}

			try
			{
				if (_flServer is null || _flServer.HasExited)
				{
					StartProcess();
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

	public IEnumerable<string> GetConsoleMessages(int page) => _consoleMessages
		.AsEnumerable()
		.Reverse()
		.Skip((page - 1) * 50)
		.Take(50);

	public int GetMessageCount() => _consoleMessages.Count;

	private void StartProcess()
	{
		if (string.IsNullOrWhiteSpace(_config.Server.FreelancerPath))
		{
			return;
		}

		var path = Environment.ExpandEnvironmentVariables(_config.Server.FreelancerPath);

		var exePath = Path.Combine(path, "EXE");
		var fileNamePath = Path.Combine(exePath, "FLServer.exe");

		if (_config.Server.CloseFLServerIfAlreadyOpen)
		{
			var activeProcessess = Process.GetProcessesByName(fileNamePath);
			if (activeProcessess is not null && activeProcessess.Length > 0)
			{
				foreach (var process in activeProcessess)
				{
					process.Close();
				}
			}
		}

		ProcessStartInfo startInfo = new()
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			Arguments = $"-noconsole /p{_config.Server.Port} /c",
			WorkingDirectory = exePath,
			FileName = fileNamePath
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
	}

	private void AddLog(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		_consoleMessages.Add(message);
		_dispatcher.Dispatch(new ConsoleMessageAction(message));
	}

	public void Start() => _readyToStart = true;
}
