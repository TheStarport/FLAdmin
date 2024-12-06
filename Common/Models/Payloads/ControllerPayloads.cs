namespace FlAdmin.Common.Models.Payloads;

public record RolePayload
{
    public string AccountId { get; set; }
    public string[] Roles { get; set; }
}