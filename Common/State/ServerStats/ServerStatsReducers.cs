namespace Common.State.ServerStats;

using Fluxor;

public static class ServerStatsReducers
{
	[ReducerMethod]
	public static ServerStatsState ReduceServerStatsUpdate(ServerStatsState _, ServerStatsAction action) => new();
}
