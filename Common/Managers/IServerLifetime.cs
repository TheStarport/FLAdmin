namespace Common.Managers;

public interface IServerLifetime
{
	void Start();
	int GetMessageCount();
	IEnumerable<string> GetConsoleMessages(int page);
	void SendCommandToConsole(string command);
}
