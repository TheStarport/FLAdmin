using MongoDB.Bson;

namespace FlAdmin.Common.Models.Payloads;

public record RolePayload
{
    public required string AccountId { get; set; }
    public required string[] Roles { get; set; }
}

public record CommandPayload
{
    public required BsonDocument Command { get; set; }
    public required Guid SessionId { get; set; }
}