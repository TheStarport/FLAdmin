using FlAdmin.Common.Auth;
using FlAdmin.Common.Services;
using FlAdmin.Configs;
using FlAdmin.DataAccess;
using FlAdmin.Logic.Services;
using FlAdmin.Logic.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        ValidIssuers = new[] {"FLAdmin"},
        IssuerSigningKey = new SymmetricSecurityKey(keyProvider.GetSigningKey()),
        TokenDecryptionKey = new SymmetricSecurityKey(keyProvider.GetSigningKey()),
        ValidateAudience = false
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<FlAdminConfig>();
builder.Services.AddSingleton<IDatabaseAccess, MongoDatabaseAccess>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<ICharacterService, CharacterService>();
builder.Services.AddSingleton<IKeyProvider, KeyProvider>(_ => keyProvider);
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
builder.Services.AddSingleton<IAuthService, AuthService>();

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