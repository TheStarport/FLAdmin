namespace FlAdmin.Common.Models;

public struct ServerDiagnosticData
{
    public long Memory;
    public int PlayerCount;
    public DateTimeOffset TimeStamp;
    public TimeSpan ServerUptime;
}