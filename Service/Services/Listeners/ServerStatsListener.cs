using Common.Messaging;
using Common.Messaging.Messages;

namespace Service.Services.Listeners;

public class ServerStatsListener : AbstractMessageListener
{
    public ServerStatsListener(IServiceProvider serviceProvider, ILogger<ServerStatsListener> logger, 
        IMessageSubscriber messageSubscriber) : base(serviceProvider, logger, messageSubscriber)
    {
    }

    protected override void Initalize(CancellationToken token)
    {
        _subscriber.Subscribe(ExchangeName.ServerStats.ToString(), async (_, ea) =>
        {
            _currentTask = HandleMessageAsync<ServerStats>(ea, ExchangeName.ServerStats.ToString(), token);
            await _currentTask;
        });
    }

    protected override Task ProcessMessageAsync<T>(T message, CancellationToken token)
    {
        ServerStats msg = (message as ServerStats)!;
        _logger.LogInformation("Testing Message");

        return Task.CompletedTask;
    }

    protected override void Shutdown() => _subscriber.Unsubscribe(ExchangeName.ServerStats.ToString());
}
