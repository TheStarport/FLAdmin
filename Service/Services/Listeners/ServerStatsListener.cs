namespace Service.Services.Listeners;
using Common.Managers;
using Common.Messaging;
using Common.Messaging.Messages;

public class ServerStatsListener : AbstractMessageListener
{
	private readonly IExchangeSubscriber _exchangeSubscriber;
	private readonly IStatsManager _statsManager;

	public ServerStatsListener(IServiceProvider serviceProvider, ILogger<ServerStatsListener> logger, IMessageSubscriber messageSubscriber, IExchangeSubscriber exchangeSubscriber,
		IStatsManager statsManager) : base(serviceProvider, logger, messageSubscriber)
	{
		_exchangeSubscriber = exchangeSubscriber;
		_statsManager = statsManager;
	}

	protected override void Initalize(CancellationToken token)
	{
		var queueName = _exchangeSubscriber.GetQueueName(ExchangeName.ServerStats.ToString());
		_exchangeSubscriber.Subscribe(ExchangeName.ServerStats.ToString(), queueName, async (_, ea) =>
		{
			_currentTask = HandleMessageAsync<ServerStats>(ea, queueName, token);
			await _currentTask;
		});
	}

	protected override Task ProcessMessageAsync<T>(T message, CancellationToken token)
	{
		var msg = (message as ServerStats)!;

		_logger.LogInformation("Updating server stats");
		_statsManager.UpdateServerStats(msg);

		return Task.CompletedTask;
	}

	protected override void Shutdown() => _subscriber.Unsubscribe(ExchangeName.ServerStats.ToString());
}
