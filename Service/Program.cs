using Blazored.LocalStorage;
using Logic.Auth;
using Logic.Managers;
using Logic.Messaging;
using Common.Auth;
using Common.Configuration;
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
using Logic.Jobs;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;

// Debug only options
#if DEBUG
IdentityModelEventSource.ShowPII = true;
#endif

var builder = WebApplication.CreateBuilder(args);

var config = FLAdminConfiguration.Get();
builder.Services.AddSingleton(config);

builder.Services.AddSingleton<IKeyProvider, KeyProvider>();
builder.Services.AddSingleton<IPersistentRoleProvider, PersistentRoleProvider>();
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
builder.Services.AddSingleton<IStatsManager, StatsManager>();
builder.Services.AddSingleton<IJobManager, JobManager>();

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

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthStateProvider>(x => x.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthenticationStateProvider>(x => x.GetRequiredService<JwtAuthStateProvider>());

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

AppDomain.CurrentDomain.ProcessExit += (_, _) => OnProcessExit(app.Services);

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

app.Run();

return 0;

// Explicit process exit as some consoles being closed do not trigger automatic shutdown steps
static void OnProcessExit(IServiceProvider services) =>
	Task.WaitAll(services.GetServices<IHostedService>().Select(service => service.StopAsync(CancellationToken.None)).ToArray());
