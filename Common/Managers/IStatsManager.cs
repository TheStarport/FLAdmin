using Common.Messaging.Messages;

namespace Common.Managers;
public interface IStatsManager
{
    uint GetServerLoad();
    string GetMemoryUsage();
    IEnumerable<PlayerInfo>? GetPlayers();
    DateTime? GetLastUpdate();
    void UpdateServerStats(ServerStats stats);
}
