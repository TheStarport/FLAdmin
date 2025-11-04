namespace FlAdmin.Common.Configs;

public class ServerConfig
{
    public string FreelancerPath { get; set; } =
        Environment.GetEnvironmentVariable("FL_PATH") ?? string.Empty;

    public int Port { get; set; } = 5577;
    
    public bool ScheduledRestartEnabled { get; set; } = true;

    public string LaunchArgs { get; set; } = "";

    public bool AutoStart { get; set; } = true;
    
    //Cron string, default time is every day at 2AM UTC. 
    public string DefaultRestartTime = "0 02 * * *";
    
}