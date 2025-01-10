namespace FlAdmin.Common.Configs;

public class ServerConfig
{
    public string FreelancerPath { get; set; } =
        Environment.GetEnvironmentVariable("FL_PATH") ?? string.Empty;
    public int Port { get; set; } = 5577;

    public String LaunchArgs { get; set; } = "";
    
    public bool AutoStart { get; set; } = true;
}