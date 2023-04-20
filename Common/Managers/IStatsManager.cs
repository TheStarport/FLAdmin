using Common.Messaging.Messages;

namespace Common.Managers;
public interface IStatsManager
{
    void UpdateServerStats(ServerStats stats);
}
