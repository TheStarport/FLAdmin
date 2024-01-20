namespace Service.Services.Listeners;
using System.Text;
using System.Text.Json;
using Common.Messaging;
using RabbitMQ.Client.Events;

public abstract class AbstractMessageListener : IHostedService
{
	protected IServiceProvider Provider { get; }
	protected ILogger Logger { get; }
	protected IMessageSubscriber Subscriber { get; }
	private JsonSerializerOptions JsonSerializerOptions { get; }

	protected Task? CurrentTask { get; set; }

	protected AbstractMessageListener(IServiceProvider serviceProvider, ILogger logger, IMessageSubscriber subscriber)
	{
		Provider = serviceProvider;
		Logger = logger;
		Subscriber = subscriber;

		JsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
	}

	protected abstract void Initialize(CancellationToken token);
	protected abstract void Shutdown();

	public Task StartAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation("Starting Message Listener: {Name}", GetType().Name);

		Initialize(cancellationToken);

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation("Stopping Message Listener: {Name}", GetType().Name);

		Shutdown();

		if (CurrentTask is not null)
		{
			await CurrentTask;
		}
	}

	protected abstract Task ProcessMessageAsync<T>(T message, CancellationToken token);

	protected async Task HandleMessageAsync<T>(BasicDeliverEventArgs ea, string queueName, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			Logger.LogInformation("Message received during shutdown");
			Subscriber.Reject(queueName, ea, requeue: true);
			Subscriber.Unsubscribe(queueName);
			return;
		}

		using var _ = Logger.BeginScope(new Dictionary<string, object>
		{
			["DeliveryTag"] = ea.DeliveryTag
		});

		Logger.LogInformation("Handling Message");

		try
		{
			var body = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.ToArray()), JsonSerializerOptions);

			await ProcessMessageAsync(body!, stoppingToken);

			Subscriber.Complete(queueName, ea);
		}
		catch (JsonException jsonException)
		{
			Subscriber.Reject(queueName, ea, false);
			Logger.LogCritical(jsonException, "Failed to deserialise message");
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Failed to process message");
			Subscriber.Reject(queueName, ea, true);
		}
	}
}
