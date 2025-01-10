using System.ComponentModel;
using System.Diagnostics;
using FlAdmin.Common.Configs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace FlAdmin.Logic.Services;

public class FlServerManager(
    ILogger<FlServerManager> logger,
    FlAdminConfig configuration)
    : BackgroundService
{
    private Process? _flServer;
    private readonly List<string> _consoleMessages = new();
    private bool _readyToStart = true;
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_readyToStart || (configuration.Server.AutoStart &&
                                   string.IsNullOrWhiteSpace(configuration.Server.FreelancerPath)))
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
                    await _flServer.WaitForExitAsync(stoppingToken);
                    _flServer.Dispose();
                    _flServer = null;
                }
            }
        }
    }

    public void SendCommandToConsole(string command) => _flServer?.StandardInput.WriteLine(command);

    public void Terminate()
    {
        _flServer?.Kill();
        _flServer?.WaitForExit();
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
        if (string.IsNullOrWhiteSpace(configuration.Server.FreelancerPath))
        {
            return false;
        }

        try
        {
            // Close FLServer if already open
            Process[] activeProcesses;
            do
            {
                activeProcesses = Process.GetProcessesByName("FLServer").Where(x => x.MainWindowTitle != "FLHook")
                    .ToArray();
                foreach (var process in activeProcesses)
                {
                    process.Kill();
                }

                await Task.Delay(TimeSpan.FromSeconds(0.5));
            } while (activeProcesses.Length > 0);
        }
        catch (Win32Exception ex)
        {
            logger.LogError(ex, "Unable to terminate existing FLServer. FLAdmin does not control active instance.");
            _readyToStart = false;
            return false;
        }

        var path = Environment.ExpandEnvironmentVariables(configuration.Server.FreelancerPath);

        var exePath = Path.Combine(path, "EXE");
        var fileNamePath = Path.Combine(exePath, "FLServer.exe");

        ProcessStartInfo startInfo = new()
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            Arguments = $"-noconsole /p{configuration.Server.Port} /c {configuration.Server.LaunchArgs}",
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
            logger.LogError(ex, "Failed to start process");
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
        logger.Log(LogLevel.Information, message);
        _consoleMessages.Add(message);
    }

    public bool IsAlive() => _flServer is not null && !_flServer.HasExited;
    public bool ReadyToStart() => _readyToStart;

    public void Start() => _readyToStart = true;
}