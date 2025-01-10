namespace FlAdmin.Common.Services;

public interface IFlServerManager
{
    public bool RestartServer(int delayInSeconds);
}