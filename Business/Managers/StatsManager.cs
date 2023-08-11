namespace Logic.Managers;

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
	private IEnumerable<PlayerInfo>? _onlinePlayers = new List<PlayerInfo>();
	private readonly Dictionary<string, uint> _playerTrends = new();

	public StatsManager(IDispatcher dispatcher) => _dispatcher = dispatcher;

	public void Clear()
	{
		_serverLoad.Clear();
		_memoryUsage.Clear();
	}

	public IEnumerable<Usage> GetMemoryUsage() => _memoryUsage;
	public IEnumerable<PlayerInfo> GetOnlinePlayers() => _onlinePlayers ?? Enumerable.Empty<PlayerInfo>();
	public IEnumerable<Load> GetServerLoad() => _serverLoad;

	public void UpdateServerStats(ServerStats stats)
	{
		_serverLoad.Add(new(stats.ServerLoad));
		_memoryUsage.Add(new(stats.MemoryUsage));
		_onlinePlayers = _onlinePlayers?
			.Where(x => stats.Players.Any(y => y.Name == x.Name))
			.Concat(stats.Players)
			.DistinctBy(x => x.Name) ?? stats.Players;
		_playerTrends.Add(DateTime.UtcNow.ToLongTimeString(), (uint)_onlinePlayers.Count());

		// Send an update indicating the state has changed
		_dispatcher.Dispatch(new ServerStatsAction());
	}

	public IEnumerable<KeyValuePair<string, uint>> GetPlayerTrend() => _playerTrends;
}
