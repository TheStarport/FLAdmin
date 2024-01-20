namespace Service.Services.Listeners;
using System.Text;
using System.Text.Json;
using Common.Messaging;
using RabbitMQ.Client.Events;

public abstract class AbstractMessageListener : IHostedService
{
	protected readonly IServiceProvider ____RULE_VIOLATION____ServiceProvider____RULE_VIOLATION____;
	protected readonly ILogger ____RULE_VIOLATION____Logger____RULE_VIOLATION____;
	protected readonly IMessageSubscriber ____RULE_VIOLATION____Subscriber____RULE_VIOLATION____;
	protected readonly JsonSerializerOptions ____RULE_VIOLATION____JsonSerializerOptions____RULE_VIOLATION____;

	protected Task? ____RULE_VIOLATION____CurrentTask____RULE_VIOLATION____;

	protected AbstractMessageListener(IServiceProvider serviceProvider, ILogger logger, IMessageSubscriber subscriber)
	{
		____RULE_VIOLATION____ServiceProvider____RULE_VIOLATION____ = serviceProvider;
		____RULE_VIOLATION____Logger____RULE_VIOLATION____ = logger;
		____RULE_VIOLATION____Subscriber____RULE_VIOLATION____ = subscriber;

		____RULE_VIOLATION____JsonSerializerOptions____RULE_VIOLATION____ = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
	}

	protected abstract void Initalize(CancellationToken token);
	protected abstract void Shutdown();

	public Task StartAsync(CancellationToken cancellationToken)
	{
		____RULE_VIOLATION____Logger____RULE_VIOLATION____.LogInformation("Starting Message Listener: {name}", GetType().Name);

		Initalize(cancellationToken);

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		____RULE_VIOLATION____Logger____RULE_VIOLATION____.LogInformation("Stopping Message Listner: {name}", GetType().Name);

		Shutdown();

		if (____RULE_VIOLATION____CurrentTask____RULE_VIOLATION____ is not null)
		{
			await ____RULE_VIOLATION____CurrentTask____RULE_VIOLATION____;
		}
	}

	protected abstract Task ProcessMessageAsync<T>(T message, CancellationToken token);

	protected async Task HandleMessageAsync<T>(BasicDeliverEventArgs ea, string queueName, CancellationToken stoppingToken)
	{
		if (stoppingToken.IsCancellationRequested)
		{
			____RULE_VIOLATION____Logger____RULE_VIOLATION____.LogInformation("Message received during shutdown");
			____RULE_VIOLATION____Subscriber____RULE_VIOLATION____.Reject(queueName, ea, requeue: true);
			____RULE_VIOLATION____Subscriber____RULE_VIOLATION____.Unsubscribe(queueName);
			return;
		}

		using var _ = ____RULE_VIOLATION____Logger____RULE_VIOLATION____.BeginScope(new Dictionary<string, object>
		{
			["DeliveryTag"] = ea.DeliveryTag
		});

		____RULE_VIOLATION____Logger____RULE_VIOLATION____.LogInformation("Handling Message");

		try
		{
			var body = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.ToArray()), ____RULE_VIOLATION____JsonSerializerOptions____RULE_VIOLATION____);

			await ProcessMessageAsync(body!, stoppingToken);

			____RULE_VIOLATION____Subscriber____RULE_VIOLATION____.Complete(queueName, ea);
		}
		catch (JsonException jsonException)
		{
			____RULE_VIOLATION____Subscriber____RULE_VIOLATION____.Reject(queueName, ea, false);
			____RULE_VIOLATION____Logger____RULE_VIOLATION____.LogCritical(jsonException, "Failed to deserialise message");
		}
		catch (Exception e)
		{
			____RULE_VIOLATION____Logger____RULE_VIOLATION____.LogError(e, "Failed to process message");
			____RULE_VIOLATION____Subscriber____RULE_VIOLATION____.Reject(queueName, ea, true);
		}
	}
}
