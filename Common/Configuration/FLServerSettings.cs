namespace Common.Configuration;

public class FLServerSettings
{
	public int Port { get; set; } = 2302;
	public bool AutoStartFLServer { get; set; } = true;
	public string FreelancerPath { get; set; } = string.Empty;
	public bool UseFLHook { get; set; } = true;
	public bool CheckForFLHookUpdates { get; set; } = true;
	public bool AutoDownloadLatestFLHook { get; set; }
	public string FLHookRepositry { get; set; } = "https://github.com/TheStarport/FLHook";
}
