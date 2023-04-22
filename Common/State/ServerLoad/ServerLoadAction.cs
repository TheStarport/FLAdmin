namespace Common.State.ServerLoad;

public class ServerLoadAction
{
	public uint ServerLoad { get; }

	public ServerLoadAction(uint serverLoad) => ServerLoad = serverLoad;
}
