using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Containers;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using Hangfire;
using LanguageExt;
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
    private readonly List<long> _flserverMemUsage = new();

    private ServerDiagnosticData _currentServerDiagnosticData;
    private Process? _flServer;
    public bool FlserverReady { get; private set; }

    //Keeps track of the past 6 hours of server diagnostics.
    public FixedSizeQueue<ServerDiagnosticData> _pastServerDiagnostics { get;} = new(360);

    private bool _readyToStart = true;
    private int _restartDelayInSeconds = 30;
    private TimeSpan _serverOnlineTime;
    private bool _shouldRestartServer;

    private DateTimeOffset _startTime;
    private int totalServerLogins;
    
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

                if (_flServer is not null) _flserverMemUsage.Add(_flServer.VirtualMemorySize64 / (1024 * 1024));

                if (!FlserverReady)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                if ((await flHookService.PingFlHook(stoppingToken)).IsSome)
                    //FLHook failed to respond after startup so we move to the catch block to restart it.
                    throw new Exception();

                if (_shouldRestartServer)
                    if (_flServer is not null && !_flServer.HasExited)
                    {
                        logger.LogInformation("Restart manually requested. Restarting.");
                        _flServer.CancelOutputRead();
                        _flServer.CancelErrorRead();
                        _flServer.Kill();
                        await _flServer.WaitForExitAsync(stoppingToken);
                        await Task.Delay(TimeSpan.FromSeconds(_restartDelayInSeconds), stoppingToken);
                        _flServer.Dispose();
                        _flServer = null;
                        FlserverReady = false;
                    }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch
            {
                if (_flServer is not null && !_flServer.HasExited)
                {
                    logger.LogInformation("FLServer stopped responding, restarting.");
                    _flServer.CancelOutputRead();
                    _flServer.CancelErrorRead();
                    _flServer.Kill();
                    await _flServer.WaitForExitAsync(stoppingToken);
                    _flServer.Dispose();
                    _flServer = null;
                    FlserverReady = false;
                }
            }
        }
    }

   /// <summary>
   /// Sets a specified period to restart the server
   /// </summary>
   /// <param name="cronString">A cron string for the restart period.</param>
   /// <param name="delay">Delay for the server restart</param>
   /// <returns></returns>
    public Option<FLAdminErrorCode> SetServerRestart(string cronString, int delay = 30)
    {
        try
        {
            RecurringJob.AddOrUpdate("ServerRestart", () => RestartServer(delay), cronString);

            return new Option<FLAdminErrorCode>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set server restart.");
            return FLAdminErrorCode.HangfireFailure;
        }
    }

    /// <summary>
    ///     Restarts the FlServer instance that FlAdmin is managing, delay is not exact and may vary a few seconds.
    /// </summary>
    /// <param name="delayInSeconds">
    ///     Default of 30 seconds if none is provided, how long FLAdmin will wait after killing the
    ///     server to start it back up.
    /// </param>
    public async Task RestartServer(int delayInSeconds = 30)
    {
        _restartDelayInSeconds = delayInSeconds;
        _shouldRestartServer = true;
        if (_flServer is not null && !_flServer.HasExited)
            await _flServer.WaitForExitAsync();
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
            FlserverReady = false;
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    //Must be public for Hangfire to execute this function.
    public async Task ServerDiagnostics()
    {
        _serverOnlineTime = DateTimeOffset.Now - _startTime;
        var memory = _flServer!.VirtualMemorySize64;
        _currentServerDiagnosticData.Memory = memory;
        _currentServerDiagnosticData.TimeStamp = DateTimeOffset.Now;
        _currentServerDiagnosticData.ServerUptime = _serverOnlineTime;
        var res = await flHookService.GetOnlineCharacters(CancellationToken.None);
        res.Match(Left: err => { }, Right: v => { _currentServerDiagnosticData.PlayerCount = v.OnlinePlayers.Count; });
        _pastServerDiagnostics.Enqueue(_currentServerDiagnosticData);
    }

    public void SendCommandToConsole(string command)
    {
        _flServer?.StandardInput.WriteLine(command);
    }

    public async Task Terminate(CancellationToken token)
    {
        _flServer?.Kill();
        await _flServer?.WaitForExitAsync(token)!;
        _readyToStart = false;
        FlserverReady = false;
        
        RecurringJob.RemoveIfExists("FLServerDiagnostic");
    }

    public Option<FLAdminErrorCode> StartServer(CancellationToken token)
    {
        if (_readyToStart)
        {
            return FLAdminErrorCode.ServerAlreadyOnline;
        }
        
        _readyToStart = true;
        return Option<FLAdminErrorCode>.None;
    }

    private async Task<bool> StartProcess()
    {
        FlserverReady = false;
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

            _startTime = DateTimeOffset.Now;
            _flServer.BeginOutputReadLine();
            _flServer.BeginErrorReadLine();
            
            RecurringJob.AddOrUpdate("FLServerDiagnostic", () => ServerDiagnostics(), Cron.Minutely);

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
        if (string.IsNullOrWhiteSpace(args.Data)) return;

        if (args.Data.Contains("FLHook Ready") && !FlserverReady) FlserverReady = true;

        try
        {
            var doc = JsonSerializer.Deserialize<FLHookLog>(args.Data);

            if (doc is null) return;

            using (LogContext.PushProperty("FLHook", true))
            {
                logger.Log(doc.GetLogLevel(), doc.ToString());
            }
        }
        catch
        {
            // ignored
        }
    }

    public TimeSpan GetServerUptime()
    {
        return _currentServerDiagnosticData.ServerUptime;
    }

    public int GetCurrentPlayerCount()
    {
        return _currentServerDiagnosticData.PlayerCount;
    }

    public long GetServerMemory()
    {
        return _currentServerDiagnosticData.Memory;
    }

    public bool IsAlive()
    {
        return _flServer is not null && !_flServer.HasExited;
    }

    public bool FlSeverIsReady()
    {
        return FlserverReady;
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