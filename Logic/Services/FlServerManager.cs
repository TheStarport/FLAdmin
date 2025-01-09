using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using FlAdmin.Common.Configs;
using FlAdmin.Common.Services;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace FlAdmin.Logic;

public class FlServerManager(ILogger<FlServerManager> logger, FlAdminConfig config) : IHostedService, IFlServerManager
{
    private readonly string _flServer = config.Server.FreelancerPath + "/EXE/FLServer.exe";
    private readonly ILogger<FlServerManager> _logger = logger;

    private Thread thread;
    private Process FLServer;
    private bool threadshouldClose;

    ConcurrentQueue<BsonDocument> _commands = new ConcurrentQueue<BsonDocument>();


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
        int timestampBefore = DateTime.Now.Millisecond;
        int timeStampAfter = DateTime.Now.Millisecond;

        while (!threadshouldClose)
        {
            ProcessCommands();

            //In order to not do excessive calculations we sleep for up to 5 seconds per loop. 
            Thread.Sleep(5000);
        }

        return;
    }

    private void ProcessCommands()
    {
        foreach (var cmd in _commands)
        {
            var command = cmd["Command"].AsString;
            if (command is "RestartFLServer")
            {
                var delay = cmd["DelayInSeconds"].AsInt32;
                ShutdownServer();
                Thread.Sleep(delay * 1000);
                StartServer();
            }
        }
    }

    private bool StartServer()
    {
        try
        {
            FLServer = new Process();

            FLServer.StartInfo.FileName = _flServer;
            FLServer.StartInfo.UseShellExecute = false;
            FLServer.StartInfo.CreateNoWindow = false;
            FLServer.StartInfo.WorkingDirectory = config.Server.FreelancerPath + "/EXE/";
            FLServer.StartInfo.Arguments = "/c";
            FLServer.Start();

            return true;
        }
        catch (Win32Exception ex)
        {
            _logger.LogError(ex, "Encountered an issue when attempting to start FLSever.exe");
            return false;
        }
    }

    private void ShutdownServer()
    {
        try
        {
            FLServer.Kill();
        }
        catch (Win32Exception ex)
        {
            _logger.LogError(ex, "Encountered an issue when attempting to shut down FLSever.exe");
        }
    }

    public bool RestartServer(int delayInSeconds)
    {
        BsonDocument doc = new BsonDocument();

        doc.Add("DelayInSeconds", delayInSeconds);
        doc.Add("Command", "RestartFLServer");

        _commands.Enqueue(doc);

        return true;
    }
}