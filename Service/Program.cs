using Business.Managers;
using Business.Messaging;
using Common.Managers;
using Common.Messaging;
using Service.Services.Listeners;

var builder = WebApplication.CreateBuilder(args);

// Messaging

builder.Services.AddSingleton<IStatsManager, StatsManager>();

if (!builder.Configuration.GetValue<bool>("DisableMessaging"))
{
    builder.Services.AddSingleton<IChannelProvider, ChannelProvider>();
    builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
    builder.Services.AddSingleton<IMessageSubscriber, MessageSubscriber>();
    builder.Services.AddHostedService<ServerStatsListener>();
}

// Frontend
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntDesign();

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