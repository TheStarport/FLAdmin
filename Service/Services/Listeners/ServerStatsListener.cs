namespace Service.Services.Listeners;
using Common.Managers;
using Common.Messaging;
using Common.Messaging.Messages;
using Logic.Extensions;
using RabbitMQ.Client;

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

	protected override void Initialize(CancellationToken token)
	{
		_exchangeSubscriber.EnsureDeclared(ExchangeName.ServerStats, ExchangeType.Fanout);

		var queueName = _exchangeSubscriber.GetQueueName(ExchangeName.ServerStats);
		_exchangeSubscriber.Subscribe(ExchangeName.ServerStats, queueName, async (_, ea) =>
		{
			CurrentTask = HandleMessageAsync<ServerStats>(ea, queueName, token);
			await CurrentTask;
		});
	}

	protected override Task ProcessMessageAsync<T>(T message, CancellationToken token)
	{
		var msg = (message as ServerStats)!;

		Logger.LogInformation("Updating server stats");
		_statsManager.UpdateServerStats(msg);

		return Task.CompletedTask;
	}

	protected override void Shutdown() => Subscriber.Unsubscribe(ExchangeName.ServerStats.ToString());
}
