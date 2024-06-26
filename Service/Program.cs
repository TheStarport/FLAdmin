using System.Diagnostics;
using Blazored.LocalStorage;
using Logic.Auth;
using Logic.Managers;
using Logic.Messaging;
using Common.Auth;
using Common.Configuration;
using Common.Freelancer;
using Common.Managers;
using Common.Messaging;
using Common.State.ServerStats;
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Logging;
using Radzen;
using Service.Services;
using Service.Services.Listeners;
using Common.Jobs;
using Common.Storage;
using Logic.Freelancer;
using Logic.Jobs;
using Logic.Storage;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Microsoft.AspNetCore.Identity;


// Debug only options
#if DEBUG
IdentityModelEventSource.ShowPII = true;
#endif

var builder = WebApplication.CreateBuilder(args);

var config = FLAdminConfiguration.Get();
builder.Services.AddSingleton(config);

builder.Services.AddSingleton<IStatsManager, StatsManager>();
builder.Services.AddSingleton<IJobManager, JobManager>();
builder.Services.AddSingleton<IFreelancerDataProvider, FreelancerDataProvider>();

// MongoDB
builder.Services.AddSingleton<IMongoManager, MongoManager>();
builder.Services.AddSingleton<IAccountStorage, AccountStorage>();
builder.Services.AddSingleton<IJobStorage, JobStorage>();

if (config.Messaging.EnableMessaging)
{
	builder.Services.AddSingleton<IExchangeSubscriber, ExchangeSubscriber>();
	builder.Services.AddSingleton<IChannelProvider, ChannelProvider>();
	builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
	builder.Services.AddSingleton<IMessageSubscriber, MessageSubscriber>();
	builder.Services.AddHostedService<ServerStatsListener>();
}

var currentAssembly = typeof(ServerStatsState).Assembly;
builder.Services.AddFluxor(options =>
{
	options.ScanAssemblies(currentAssembly);
	options.WithLifetime(StoreLifetime.Singleton);
});

builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(x =>
{
	x.WaitForJobsToComplete = true;
	x.AwaitApplicationStarted = true;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthenticationCore();
builder.Services.AddBlazoredLocalStorage();

// Radzen components
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();

// Authentication
builder.Services.AddSingleton<IKeyProvider, KeyProvider>();
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();

builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>((x) => x.GetRequiredService<AuthStateProvider>());

// Hosted Services
builder.Services.AddSingleton<IServerLifetime, ServerLifetimeService>();
builder.Services.AddHostedService(x => (x.GetRequiredService<IServerLifetime>() as ServerLifetimeService)!);

// Extend the shutdown timer to allow FLServer time to gracefully shutdown
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(10));

// Handle application logging
builder.Host.UseSerilog((_, lc) =>
{
	var logLevel = config.Logging.EnableDebugLogs ? LogEventLevel.Debug : LogEventLevel.Information;
	lc.Enrich.FromLogContext();
	lc.WriteTo.Console(new CompactJsonFormatter(), logLevel);

	// FLAdmin only logs
	lc.WriteTo.Logger(x =>
	{
		var excluding = x.Filter.ByExcluding(Matching.WithProperty("FLHook"));
		SetupLogConfiguration(excluding, "FLAdmin", config.Logging.LogFileFLAdmin);
	});

	// FLHook only logs
	lc.WriteTo.Logger(x =>
	{
		var including = x.Filter.ByIncludingOnly(Matching.WithProperty("FLHook"));
		SetupLogConfiguration(including, "FLHook", config.Logging.LogFileFLHook);
	});

	// ReSharper disable once SeparateLocalFunctionsWithJumpStatement
	void SetupLogConfiguration(LoggerConfiguration logger, string tag, string? logFile)
	{
		if (!string.IsNullOrWhiteSpace(logFile))
		{
			logger.WriteTo.File(new CompactJsonFormatter(), logFile, logLevel);
		}

		if (!config.Logging.FluentDOptions.Enable)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(config.Logging.FluentDOptions.UnixSocket))
		{
			logger.WriteTo.Fluentd(config.Logging.FluentDOptions.Host, config.Logging.FluentDOptions.Port, tag: tag, restrictedToMinimumLevel: logLevel);
		}
		else
		{
			logger.WriteTo.Fluentd(config.Logging.FluentDOptions.UnixSocket, logLevel);
		}
	}
});

var app = builder.Build();

// If we can, load freelancer data, connect to external services, etc

if (!await app.Services.GetRequiredService<IMongoManager>().ConnectAsync())
{
	app.Logger.LogCritical("Unable to connect to MongoDB. Please check credentials! Configuration file can be found at {Path}", FLAdminConfiguration.ConfigPath);
	Debugger.Break();
	return -1;
}

app.Services.GetRequiredService<IFreelancerDataProvider>().Reload();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseAuthentication();
app.UseAuthorization();

app.Run();

return 0;
