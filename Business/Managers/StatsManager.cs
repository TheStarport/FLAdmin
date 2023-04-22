namespace Business.Managers;

using System.Collections.Generic;
using Common.Managers;
using Common.Messaging.Messages;
using Common.State.ServerStats;
using Fluxor;

public class StatsManager : IStatsManager
{
	private readonly IDispatcher _dispatcher;
	private readonly IList<Load> _serverLoad = new List<Load>();
	private readonly IList<Usage> _memoryUsage = new List<Usage>();

	public StatsManager(IDispatcher dispatcher) => _dispatcher = dispatcher;

	public void Clear()
	{
		_serverLoad.Clear();
		_memoryUsage.Clear();
	}

	public IEnumerable<Usage> GetMemoryUsage() => _memoryUsage;
	public IEnumerable<Load> GetServerLoad() => _serverLoad;

	public void UpdateServerStats(ServerStats stats)
	{
		_serverLoad.Add(new(stats.ServerLoad));
		_memoryUsage.Add(new(stats.MemoryUsage));

		// Send an update indicating the state has changed
		_dispatcher.Dispatch(new ServerStatsAction());
	}
}
