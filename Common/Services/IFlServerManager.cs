namespace FlAdmin.Common.Services;

public interface IFlServerManager
{
    public Task RestartServer(int delayInSeconds);

    public bool IsAlive();
    
    public bool FlSeverIsReady();
    
    
}