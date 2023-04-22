namespace Business.Managers;
using Common.Managers;
using Common.Messaging.Messages;
using Common.State.ServerLoad;
using Fluxor;

public class StatsManager : IStatsManager
{
	private readonly IDispatcher _dispatcher;

	public StatsManager(IDispatcher dispatcher) => _dispatcher = dispatcher;

	public static string GetMemoryUsage() =>
		/*string[] suffix = { "B", "KB", "MB", "GB" };
int i;
double dblSByte = MemoryUsage;
for (i = 0; i < suffix.Length && MemoryUsage >= 1024; i++, MemoryUsage /= 1024)
{
dblSByte = MemoryUsage / 1024.0;
}

return string.Format("{0:0.##} {1}", dblSByte, suffix[i]);*/
		"";

	public void UpdateServerStats(ServerStats stats) =>
		/*MemoryUsage = stats.MemoryUsage;
ServerLoad = stats.ServerLoad;
OnlinePlayers = OnlinePlayers?
.Where(x => stats.Players.Any(y => y.Name == x.Name))
.Concat(stats.Players)
.DistinctBy(x => x.Name) ?? stats.Players;
LastUpdated = DateTime.UtcNow;*/

		_dispatcher.Dispatch(new ServerLoadAction(stats.ServerLoad));
}
