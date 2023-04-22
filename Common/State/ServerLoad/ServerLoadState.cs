namespace Common.State.ServerLoad;
using Fluxor;

[FeatureState]
public class ServerLoadState
{
	public uint ServerLoadMs { get; }

	public ServerLoadState()
	{

	}

	public ServerLoadState(uint serverLoadMs) => ServerLoadMs = serverLoadMs;
}
