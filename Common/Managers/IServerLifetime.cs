namespace Common.Managers;

public interface IServerLifetime
{
	int GetMessageCount();
	IEnumerable<string> GetConsoleMessages(int page);
	void AddLog(string message);
	void SendCommandToConsole(string command);
}
