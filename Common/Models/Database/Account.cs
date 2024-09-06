using System.Security.Claims;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlAdmin.Common.Models.Database;

public class Account
{
    [BsonId] public required string Id { get; set; }

    [BsonElement("characters")] public List<ObjectId> Characters { get; set; } = [];
    [BsonElement("scheduledUnbanDate")] public DateTimeOffset? ScheduledUnbanDate { get; set; }
    [BsonElement("gameRoles")] public List<string> GameRoles { get; set; } = new();
    [BsonElement("webRoles")] public List<string> WebRoles { get; set; } = new();
    [BsonElement("cash")] public long Cash { get; set; }
    [BsonElement("username")] public string? Username { get; set; }
    [BsonElement("password")] public string? PasswordHash { get; set; }
    [BsonElement("salt")] public byte[]? Salt { get; set; }
    [BsonElement("lastOnline")] public DateTimeOffset? LastOnline { get; set; }

    public bool IsGameAdmin => GameRoles.Count is not 0;
    public bool HasWebAccess => WebRoles.Count is not 0;
    
    public bool Banned => ScheduledUnbanDate is null;


    public ClaimsPrincipal ToClaimsPrincipal()
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, Id), new(ClaimTypes.Name, Username!),
                    new(ClaimTypes.Hash, PasswordHash!)
                }.Concat(WebRoles.Select(x => new Claim(ClaimTypes.Role, x))), "fladmin"));
    }

    public static Account? FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        return principal.Claims.Count() is not 0
            ? new Account
            {
                Id = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                Username = principal.FindFirst(ClaimTypes.Name)!.Value,
                PasswordHash = principal.FindFirst(ClaimTypes.Hash)!.Value,
                WebRoles = principal.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            }
            : null;
    }
}