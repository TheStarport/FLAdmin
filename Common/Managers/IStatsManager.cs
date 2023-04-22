namespace Common.Managers;
using Common.Messaging.Messages;

public interface IStatsManager
{
	void UpdateServerStats(ServerStats stats);
}
