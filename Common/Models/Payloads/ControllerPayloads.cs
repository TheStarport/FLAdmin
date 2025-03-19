namespace FlAdmin.Common.Models.Payloads;

public record RolePayload
{
    public required string AccountId { get; set; }
    public required string[] Roles { get; set; }
}