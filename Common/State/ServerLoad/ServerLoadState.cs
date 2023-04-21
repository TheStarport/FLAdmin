using Fluxor;

namespace Common.State.ServerLoad;

[FeatureState]
public class ServerLoadState
{
    public uint ServerLoadMs { get; }

    public ServerLoadState(uint serverLoadMs)
    {
        ServerLoadMs = serverLoadMs;
    }
}
