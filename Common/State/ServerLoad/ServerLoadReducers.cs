namespace Common.State.ServerLoad;
using Fluxor;

public static class ServerLoadReducers
{
	[ReducerMethod]
	public static ServerLoadState ReduceServerLoadUpdate(ServerLoadState _, ServerLoadAction action) => new(action.ServerLoad);
}
