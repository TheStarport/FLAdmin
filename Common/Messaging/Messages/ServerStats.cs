namespace Common.Messaging.Messages;
using System.Text.Json.Serialization;

public class ServerStats
{
	public uint MemoryUsage { get; set; }
	public uint ServerLoad { get; set; }
	public PlayerInfo[] Players { get; set; } = Array.Empty<PlayerInfo>();
}

public class PlayerInfo
{
	public string IpAddress { get; set; } = string.Empty;
	[JsonPropertyName("PlayerName")]
	public string Name { get; set; } = string.Empty;
	public string SystemName { get; set; } = string.Empty;
	[JsonPropertyName("SystemNick")]
	public string SystemNickname { get; set; } = string.Empty;
}
