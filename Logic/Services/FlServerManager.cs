using System.ComponentModel;
using System.Diagnostics;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace FlAdmin.Logic.Services;

public class FlServerManager(
    ILogger<FlServerManager> logger,
    FlAdminConfig configuration,
    IFlHookService flHookService)
    : BackgroundService
{
    private readonly List<string> _consoleMessages = new();
    private Process? _flServer;
    private List<long> _flserverMemUsage;
    private bool _flserverReady;
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
                if ((_flServer is null || _flServer.HasExited) && !await StartProcess()) continue;
                if (!_flserverReady)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }
                
                

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch
            {
                if (_flServer is not null && !_flServer.HasExited)
                {
                    _flServer.CancelOutputRead();
                    _flServer.CancelErrorRead();
                    _flServer.Kill();
                    await _flServer.WaitForExitAsync(stoppingToken);
                    _flServer.Dispose();
                    _flServer = null;
                    _flserverReady = false;
                }
            }
        }
    }

    public override void Dispose()
    {
        if (_flServer is not null && !_flServer.HasExited)
        {
            _flServer.CancelOutputRead();
            _flServer.CancelErrorRead();
            _flServer.Kill();
            _flServer.Dispose();
            _flServer = null;
            _flserverReady = false;
        }
    }

    private void ServerDiagnostics()
    {
        var memory = _flServer!.VirtualMemorySize64;
        _flserverMemUsage.Add(memory);
    }

    public void SendCommandToConsole(string command)
    {
        _flServer?.StandardInput.WriteLine(command);
    }

    public void Terminate()
    {
        _flServer?.Kill();
        _flServer?.WaitForExit();
        _readyToStart = false;
        _flserverReady = false;
    }

    public IEnumerable<string> GetConsoleMessages(int page)
    {
        return _consoleMessages
            .AsEnumerable()
            .Reverse()
            .Skip((page - 1) * 50)
            .Take(50);
    }

    public int GetMessageCount()
    {
        return _consoleMessages.Count;
    }

    private async Task<bool> StartProcess()
    {
        _flserverReady = false;
        if (string.IsNullOrWhiteSpace(configuration.Server.FreelancerPath)) return false;

        try
        {
            // Close FLServer if already open
            Process[] activeProcesses;
            do
            {
                activeProcesses = Process.GetProcessesByName("FLServer")
                    .ToArray();
                foreach (var process in activeProcesses) process.Kill();

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
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            Arguments = $"-noconsole /p{configuration.Server.Port} /c {configuration.Server.LaunchArgs}",
            WorkingDirectory = exePath,
            FileName = fileNamePath
        };

        try
        {
            _flServer = new Process();

            _flServer.ErrorDataReceived += AddLog;
            _flServer.OutputDataReceived += AddLog;

            _flServer.StartInfo = startInfo;
            _flServer.Start();
            if (_flServer is null) throw new InvalidOperationException("Unable to start FLServer.exe");


            _flServer.BeginOutputReadLine();
            _flServer.BeginErrorReadLine();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start process");
            // Sleep as to not overwhelm with attempts
            await Task.Delay(TimeSpan.FromSeconds(10));
            return false;
        }
    }

    private void AddLog(object sendingProcess, DataReceivedEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Data))
        {
            return;
        }

        if (args.Data.Contains("FLHook Ready"))
        {
            _flserverReady = true;
        }

        using var prop = LogContext.PushProperty("FLHook", true);

        logger.Log(LogLevel.Information, args.Data);
        _consoleMessages.Add(args.Data);
    }

    public bool IsAlive()
    {
        return _flServer is not null && !_flServer.HasExited;
    }

    public bool ReadyToStart()
    {
        return _readyToStart;
    }

    public void Start()
    {
        _readyToStart = true;
    }
}