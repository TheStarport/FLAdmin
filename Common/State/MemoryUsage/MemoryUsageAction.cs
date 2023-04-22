namespace Common.State.MemoryUsage;

public class MemoryUsageAction
{
	public uint MemoryUsage { get; }

	public MemoryUsageAction(uint memoryUsage) => MemoryUsage = memoryUsage;
}
