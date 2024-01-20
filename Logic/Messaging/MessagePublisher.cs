namespace Logic.Messaging;
using System.Text;
using System.Text.Json;
using Common.Messaging;
using Microsoft.Extensions.Logging;

public class MessagePublisher : IMessagePublisher
{
	private readonly ILogger<MessagePublisher> _logger;
	private readonly IChannelProvider _channelProvider;

	public MessagePublisher(ILogger<MessagePublisher> logger, IChannelProvider channelProvider)
	{
		_logger = logger;
		_channelProvider = channelProvider;
	}

	public void Publish<T>(QueueName queue, T message) => Publish("", queue.ToString(), message);

	public void Publish<T>(ExchangeName exchange, T message) => Publish(exchange.ToString(), "", message);

	private void Publish<T>(string exchange, string queue, T message)
	{
		ReadOnlyMemory<byte> bodyJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
		var channel = _channelProvider!.ProvideChannel(queue);

		if (channel is null)
		{
			return;
		}

		var messageProperties = channel.CreateBasicProperties();
		messageProperties.Headers ??= new Dictionary<string, object>();

		channel.BasicPublish(exchange: exchange, routingKey: queue, basicProperties: messageProperties, body: bodyJson, mandatory: false);
		_logger.LogInformation("Sent RabbitMq Message");
	}
}
