using System.ComponentModel;
using System.Diagnostics;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Services;
using LanguageExt.Pipes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic;

public class FlServerManager(ILogger<FlServerManager> logger, FlAdminConfig config) : IHostedService, IFlServerManager
{
    private readonly string _flServer = config.Server.FreelancerPath + "/EXE/FLServer.exe";
    private readonly ILogger<FlServerManager> _logger = logger;

    private Thread thread;

    private bool threadshouldClose;

    private Process FLServer;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!StartServer())
        {
            return Task.CompletedTask;
        }
        
        thread = new Thread(ServerTask);
        thread.IsBackground = true;
        threadshouldClose = false;
        thread.Start();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        threadshouldClose = true;
        ShutdownServer();
        thread.Join();
        return Task.CompletedTask;
    }
    
    private void ServerTask()
    {
        while (!threadshouldClose)
        {
             Thread.Sleep(1000);
        }

        return;
    }

    private bool StartServer()
    {
        try
        {
            FLServer = new Process();

            FLServer.StartInfo.FileName = _flServer;
            FLServer.StartInfo.UseShellExecute = false;
            FLServer.StartInfo.CreateNoWindow = false;
            FLServer.Start();

            return true;
        }
        catch (Win32Exception ex)
        {
            _logger.LogError(ex,"Encountered an issue when attempting to start FLSever.exe");
            return false;
        }
    }

    private void ShutdownServer()
    {
        FLServer.Kill();
    }
    
    public bool RestartServer()
    {
        throw new NotImplementedException();
    }


}