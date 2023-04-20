using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Managers;
using Common.Messaging.Messages;

namespace Business.Managers;
public class StatsManager : IStatsManager
{
    public uint MemoryUsage { get; private set; }
    public uint ServerLoad { get; private set; }
    public IEnumerable<PlayerInfo>? OnlinePlayers { get; private set; }
    public DateTime? LastUpdated { get; private set; }

    public void UpdateServerStats(ServerStats stats)
    {
        MemoryUsage = stats.MemoryUsage;
        ServerLoad = stats.ServerLoad;
        OnlinePlayers = OnlinePlayers?
            .Where(x => stats.Players.Any(y => y.Name == x.Name))
            .Concat(stats.Players)
            .DistinctBy(x => x.Name) ?? stats.Players;
        LastUpdated = DateTime.UtcNow;
    }
}
