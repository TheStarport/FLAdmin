using Common.Messaging;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Service.Services.Listeners;

public abstract class AbstractMessageListener : IHostedService
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger _logger;
    protected readonly IMessageSubscriber _subscriber;
    protected readonly JsonSerializerOptions _jsonSerializerOptions;

    protected Task? _currentTask;

    public AbstractMessageListener(IServiceProvider serviceProvider, ILogger logger, IMessageSubscriber subscriber)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _subscriber = subscriber;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected abstract void Initalize(CancellationToken token);
    protected abstract void Shutdown();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Message Listener: {name}", GetType().Name);

        Initalize(cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Message Listner: {name}", GetType().Name);

        Shutdown();

        if (_currentTask is not null)
        {
            await _currentTask;
        }
    }

    protected abstract Task ProcessMessageAsync<T>(T message, CancellationToken token);

    protected async Task HandleMessageAsync<T>(BasicDeliverEventArgs ea, string queueName, CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Message received during shutdown");
            _subscriber.Reject(queueName, ea, requeue: true);
            _subscriber.Unsubscribe(queueName);
            return;
        }

        using var _ = _logger.BeginScope(new Dictionary<string, object>
        { 
            ["DeliveryTag"] = ea.DeliveryTag
        });

        _logger.LogInformation("Handling Message");

        try
        {
            var body = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.ToArray()), _jsonSerializerOptions);

            await ProcessMessageAsync<T>(body!, stoppingToken);

            _subscriber.Complete(queueName, ea);
        }
        catch (JsonException jsonException)
        {
            _subscriber.Reject(queueName, ea, false);
            _logger.LogCritical(jsonException, "Failed to deserialise message");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process message");
            _subscriber.Reject(queueName, ea, true);
        }
    }
}
