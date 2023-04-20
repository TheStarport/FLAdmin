namespace Common.Messaging.Messages;
public class ServerStats
{
    public uint MemoryUsage { get; set; }
    public uint ServerLoad { get; set; }
    public PlayerInfo[] Players { get; set; } = Array.Empty<PlayerInfo>();
}

public class PlayerInfo
{
    public string IpAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string SystemNickname { get; set; } = string.Empty;
}