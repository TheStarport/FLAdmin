namespace Common.State.ServerStats;

public class Load
{
	public uint MS { get; }
	public string Time { get; }

	public Load(uint ms)
	{
		MS = ms;
		Time = DateTime.UtcNow.ToLongTimeString();
	}
}
