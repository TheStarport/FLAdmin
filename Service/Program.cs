using Business.Managers;
using Business.Messaging;
using Common.Managers;
using Common.Messaging;
using Fluxor;
using Service.Services.Listeners;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStatsManager, StatsManager>();

if (!builder.Configuration.GetValue<bool>("DisableMessaging"))
{
    builder.Services.AddSingleton<IExchangeSubscriber, ExchangeSubscriber>();
    builder.Services.AddSingleton<IChannelProvider, ChannelProvider>();
    builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
    builder.Services.AddSingleton<IMessageSubscriber, MessageSubscriber>();
    builder.Services.AddHostedService<ServerStatsListener>();
}

// Frontend
var currentAssembly = typeof(Program).Assembly;
builder.Services.AddFluxor(options => options.ScanAssemblies(currentAssembly));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

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