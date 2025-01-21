using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Common.Models;

public class FLHookLog
{
    [JsonPropertyName ("function")]
    private string _function { get; set; }  = "";
    
    [JsonPropertyName ("level")]
    private FlLogLevel _LogLevel { get; set; }
    
    [JsonPropertyName ("valueMap")]
    private Dictionary<string,string> _valueMap { get; set; } = new Dictionary<string, string>();
    
    [JsonPropertyName ("message")]
    private string _message { get; set; } = "";
    
    [JsonPropertyName ("logTime")]
    private long _logTime;


    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public LogLevel GetLogLevel()
    {
        return _LogLevel switch
        {
            FlLogLevel.Debug => LogLevel.Debug,
            FlLogLevel.Error => LogLevel.Error,
            FlLogLevel.Trace => LogLevel.Trace,
            FlLogLevel.Info => LogLevel.Information,
            FlLogLevel.Warn => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}

enum FlLogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error
}