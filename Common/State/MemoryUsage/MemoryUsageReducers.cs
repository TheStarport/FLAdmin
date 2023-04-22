namespace Common.State.MemoryUsage;

using Fluxor;

public static class MemoryUsageReducers
{
	[ReducerMethod]
	public static MemoryUsageState ReduceMemoryUsageUpdate(MemoryUsageState _, MemoryUsageAction action) => new(action.MemoryUsage);
}
