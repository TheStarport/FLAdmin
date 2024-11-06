using FlAdmin.Common.Auth;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Services;
using FlAdmin.Logic.DataAccess;
using FlAdmin.Logic.Services;
using FlAdmin.Logic.Services.Auth;
using FlAdmin.Logic.Services.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

var config = new FlAdminConfig();
var keyProvider = new KeyProvider();


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuers = ["FLAdmin"],
        IssuerSigningKey = new SymmetricSecurityKey(keyProvider.GetSigningKey()),
        TokenDecryptionKey = new SymmetricSecurityKey(keyProvider.GetSigningKey()),
        ValidateAudience = false
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

//Config
builder.Services.AddSingleton<FlAdminConfig>();

//Data Access
builder.Services.AddSingleton<IDatabaseAccess, MongoDatabaseAccess>();
builder.Services.AddSingleton<IAccountDataAccess, AccountDataAccess>();
builder.Services.AddSingleton<ICharacterDataAccess, CharacterDataAccess>();
builder.Services.AddSingleton<IFreelancerDataProvider,FreelancerDataProvider>();

//Authentication
builder.Services.AddSingleton<IKeyProvider, KeyProvider>(_ => keyProvider);
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();

//Services
builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<ICharacterService, CharacterService>();
builder.Services.AddSingleton<IAuthService, AuthService>();

// Extend the shutdown timer to allow FLServer time to gracefully shutdown
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(10));

builder.Host.UseSerilog((_, lc) =>
{
    lc.Enrich.FromLogContext();
    if (config.Logging.LoggingLocation == LoggingLocation.Console)
    {
        lc.WriteTo.Console(new CompactJsonFormatter(), LogEventLevel.Information);
    }
});

builder.Services.AddControllers();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Server Online.");
app.MapControllers();

app.Run();