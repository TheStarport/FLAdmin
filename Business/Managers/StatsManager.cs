namespace Business.Managers;
using Common.Managers;
using Common.Messaging.Messages;
using Common.State.MemoryUsage;
using Common.State.ServerLoad;
using Fluxor;

public class StatsManager : IStatsManager
{
	private readonly IDispatcher _dispatcher;

	public StatsManager(IDispatcher dispatcher) => _dispatcher = dispatcher;

	public void UpdateServerStats(ServerStats stats)
	{
		_dispatcher.Dispatch(new ServerLoadAction(stats.ServerLoad));
		_dispatcher.Dispatch(new MemoryUsageAction(stats.MemoryUsage));
	}
}
