namespace FlAdmin.Common.Configs;

public enum LoggingLocation
{
    Console,
    File,
    Aggregator
}
public class LoggingConfig
{
    public LoggingLocation LoggingLocation { get; set; } = LoggingLocation.Console;
    public string LogFilePath { get; set; } = "/logs.txt";
    
}