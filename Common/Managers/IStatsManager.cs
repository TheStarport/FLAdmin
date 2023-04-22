namespace Common.Managers;
using Common.Messaging.Messages;
using Common.State.ServerStats;

public interface IStatsManager
{
	void UpdateServerStats(ServerStats stats);
	IEnumerable<Usage> GetMemoryUsage();
	IEnumerable<Load> GetServerLoad();
	IEnumerable<PlayerInfo> GetOnlinePlayers();
	IEnumerable<KeyValuePair<string, uint>> GetPlayerTrend();
	void Clear();
}
