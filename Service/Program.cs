using System.Security.AccessControl;
using System.Security.Principal;
using Blazored.LocalStorage;
using Business.Auth;
using Business.Managers;
using Business.Messaging;
using Common.Auth;
using Common.Managers;
using Common.Messaging;
using Common.State.ServerStats;
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Logging;
using Radzen;
using Service;
using Service.Services;
using Service.Services.Listeners;

if (!OperatingSystem.IsWindows())
{
	throw new InvalidOperationException("FLAdmin is meant to be run on Windows only.");
}

// Debug only options
#if DEBUG
IdentityModelEventSource.ShowPII = true;
#endif

var builder = WebApplication.CreateBuilder(args);

var goodToGo = PrelaunchChecks(builder.Configuration);
if (goodToGo is not ErrorReason.NoError)
{
	Environment.Exit((int)goodToGo);
}

builder.Services.AddSingleton<IKeyProvider, KeyProvider>();
builder.Services.AddSingleton<IPersistantRoleProvider, PersistantRoleProvider>();
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
builder.Services.AddSingleton<IStatsManager, StatsManager>();

if (!builder.Configuration.GetValue<bool>("DisableMessaging"))
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

var app = builder.Build();

AppDomain.CurrentDomain.ProcessExit += new EventHandler((_, _) => OnProcessExit(app.Services));

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

static bool DirectoryHasPermission(string directoryPath, FileSystemRights accessRight)
{
	if (string.IsNullOrEmpty(directoryPath))
	{
		return false;
	}

	try
	{
		var rules = new DirectoryInfo(directoryPath).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
		var identity = WindowsIdentity.GetCurrent();
		foreach (FileSystemAccessRule rule in rules)
		{
			if (identity.Groups!.Contains(rule.IdentityReference) && (accessRight & rule.FileSystemRights) == accessRight && rule.AccessControlType == AccessControlType.Allow)
			{
				return true;
			}
		}
	}
	catch
	{
		// No need to handle
	}
	return false;
}

static ErrorReason PrelaunchChecks(IConfiguration config)
{
	var flDirectory = Environment.ExpandEnvironmentVariables(config.GetValue<string>("FreelancerPath") ?? "");

	if (!DirectoryHasPermission(flDirectory, FileSystemRights.FullControl) ||
		!DirectoryHasPermission(Path.Combine(flDirectory, "EXE"), FileSystemRights.FullControl))
	{
		return ErrorReason.MissingDirectoryPermissions;
	}

	if (!File.Exists(Path.Combine(flDirectory, "EXE/FLServer.exe")))
	{
		return ErrorReason.FLServerNotFound;
	}

	return ErrorReason.NoError;
}

// Explicit process exit as some consoles being closed do not trigger automatic shutdown steps
static void OnProcessExit(IServiceProvider services)
{
	List<Task> shutdownTasks = new();
	foreach (var service in services.GetServices<IHostedService>())
	{
		shutdownTasks.Add(service.StopAsync(CancellationToken.None));
	}

	Task.WaitAll(shutdownTasks.ToArray());
}
