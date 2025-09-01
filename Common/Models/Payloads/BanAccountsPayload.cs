namespace FlAdmin.Common.Models.Payloads;

public struct BanAccountsPayload
{
    public List<PlayerBan> Bans;
}


public struct PlayerBan
{
    public string AccountId;
    public TimeSpan? Duration;
}