namespace Common.Configuration;

[Serializable]
public class MessagingSettings
{
	public bool EnableMessaging { get; set; } = true;
	public string Username { get; set; } = "guest";
	public string Password { get; set; } = "guest";
	public int Port { get; set; } = 5672;
	public string HostName { get; set; } = "localhost";
}
