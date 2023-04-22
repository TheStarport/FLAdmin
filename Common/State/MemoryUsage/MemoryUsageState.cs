namespace Common.State.MemoryUsage;
using Fluxor;

[FeatureState]
public class MemoryUsageState
{
	public uint MemoryUsageBytes { get; }

	public MemoryUsageState()
	{

	}

	public MemoryUsageState(uint memoryUsageBytes) => MemoryUsageBytes = memoryUsageBytes;
}
