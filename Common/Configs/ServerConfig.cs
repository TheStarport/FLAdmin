namespace FlAdmin.Common.Configs;

public class ServerConfig
{
    public string FreelancerPath { get; set; } =
        Environment.GetEnvironmentVariable("FL_PATH") ?? string.Empty;
}