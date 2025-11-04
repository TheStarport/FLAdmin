using FlAdmin.Common.Auth;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Services;
using FlAdmin.Logic.DataAccess;
using FlAdmin.Logic.Services;
using FlAdmin.Logic.Services.Auth;
using FlAdmin.Logic.Services.Database;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var config = new FlAdminConfig();


if (File.Exists("fladmin.json"))
{
    config = JsonSerializer.Deserialize<FlAdminConfig>(File.ReadAllText("fladmin.json"));
}
else
{
    var json = JsonSerializer.Serialize(config);
    File.WriteAllText("fladmin.json", json);
}

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
builder.Services.AddSingleton(config);

//Data Access
builder.Services.AddSingleton<IDatabaseAccess, MongoDatabaseAccess>();
builder.Services.AddSingleton<IAccountDataAccess, AccountDataAccess>();
builder.Services.AddSingleton<ICharacterDataAccess, CharacterDataAccess>();
builder.Services.AddSingleton<IFreelancerDataProvider, FreelancerDataProvider>();

//Authentication
builder.Services.AddSingleton<IKeyProvider, KeyProvider>(_ => keyProvider);
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();

//Services
builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<ICharacterService, CharacterService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IFlHookService, FlHookService>();
builder.Services.AddSingleton<IConfigService, ConfigService>();

//Background Services
builder.Services.AddHostedService<FlServerManager>();

// Extend the shutdown timer to allow FLServer time to gracefully shutdown
builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(10));

//Logging
builder.Host.UseSerilog((_, lc) =>
{
    lc.Enrich.FromLogContext();
    if (config.Logging.LoggingLocation == LoggingLocation.Console)
        lc.WriteTo.MongoDBBson(
            databaseUrl: config.Mongo.ConnectionString + "/" + config.Mongo.DatabaseName,
            collectionName: config.Mongo.FlAdminLogCollectionName);
});

//Configure Hangfire to use MongoDB
var mongoConnection = config.Mongo.ConnectionString;

builder.Services.AddHangfire(configuration =>
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMongoStorage(connectionString: config.Mongo.ConnectionString,
            databaseName: config.Mongo.JobDatabaseName, new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                },
                CheckConnection = true
            }));

builder.Services.AddHangfireServer(options => options.ServerName = "FLAdmin");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerGenOptions =>
{
    swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "FlAdmin API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(swaggerGenOptions =>
    {
        swaggerGenOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "FlAdmin API");
    });
}

app.UseHangfireDashboard();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Server Online.");
app.MapControllers();

app.Run();