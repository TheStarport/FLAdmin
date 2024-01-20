namespace Logic.Messaging;

using Common.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

public class ExchangeSubscriber : IExchangeSubscriber
{
	private readonly Dictionary<string, string> _exchangeQueuePair = new();
	private readonly IChannelProvider _channelProvider;
	private readonly IMessageSubscriber _messageSubscriber;
	private readonly ILogger<ExchangeSubscriber> _logger;

	public ExchangeSubscriber(IChannelProvider channelProvider, IMessageSubscriber messageSubscriber, ILogger<ExchangeSubscriber> logger)
	{
		_channelProvider = channelProvider;
		_messageSubscriber = messageSubscriber;
		_logger = logger;
	}

	public string GetQueueName(string exchangeName)
	{
		var queueName = $"{exchangeName}-{Guid.NewGuid()}";
		if (_exchangeQueuePair.TryGetValue(exchangeName, out var value))
		{
			queueName = value;
		}
		else
		{
			_exchangeQueuePair[exchangeName] = exchangeName;
		}

		return queueName;
	}

	public void Subscribe(string exchangeName, string queueName, AsyncEventHandler<BasicDeliverEventArgs> action)
	{
		var channel = _channelProvider.ProvideChannel(queueName);

		if (channel is null)
		{
			return;
		}

		var queue = channel.QueueDeclare(queueName, false, true, true, new Dictionary<string, object>());
		channel.QueueBind(queue.QueueName, exchangeName, string.Empty, new Dictionary<string, object>());

		_messageSubscriber.Subscribe(queue.QueueName, action);
	}

	public void Unsubscribe(string exchangeName)
	{
		_logger.LogInformation("Unsubscribing from exchange: {}", exchangeName);
		foreach (var queue in _exchangeQueuePair)
		{
			_messageSubscriber.Unsubscribe(queue.Value);
		}
	}
}
