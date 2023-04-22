namespace Common.State.ServerStats;

public class Usage
{
	public uint UsageBytes { get; }
	public string Time { get; }

	public double UsageDenomination
	{
		get
		{
			var bytes = UsageBytes;
			int i;
			double dblSByte = bytes;
			for (i = 0; i < 3 && bytes >= 1024; i++, bytes /= 1024)
			{
				dblSByte = bytes / 1024.0;
			}

			return dblSByte;
		}
	}

	public Usage(uint usage)
	{
		UsageBytes = usage;
		Time = DateTime.UtcNow.ToLongTimeString();
	}
}
