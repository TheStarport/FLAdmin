namespace Logic.Messaging;
using Common.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public sealed class MessageSubscriber : IMessageSubscriber
{
	private readonly IChannelProvider _channelProvider;
	private readonly ILogger<MessageSubscriber> _logger;

	public MessageSubscriber(IChannelProvider channelProvider, ILogger<MessageSubscriber> logger)
	{
		_channelProvider = channelProvider;
		_logger = logger;
	}

	public void Subscribe(string queueName, AsyncEventHandler<BasicDeliverEventArgs> action)
	{
		var channel = _channelProvider.ProvideChannel(queueName);

		channel.BasicQos(0, 1, false);
		var consumer = new AsyncEventingBasicConsumer(channel);

		consumer.Received += action;
		consumer.Shutdown += (_, e) =>
		{
			if (e.ReplyCode != 200)
			{
				_logger.LogCritical("Service has been forcibly disconnected from the message broker.");
			}
			return Task.CompletedTask;
		};

		channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
	}

	public void Unsubscribe(string queueName)
	{
		var channel = _channelProvider.ProvideChannel(queueName);
		channel.Close();
	}

	public void Reject(string queueName, BasicDeliverEventArgs ea, bool requeue)
	{
		var channel = _channelProvider.ProvideChannel(queueName) ??
			throw new NotSupportedException("RabbitMQ connection has not been established");
		channel.BasicReject(ea.DeliveryTag, requeue);
	}

	public void Complete(string queueName, BasicDeliverEventArgs ea)
	{
		var channel = _channelProvider.ProvideChannel(queueName)
			?? throw new NotSupportedException("RabbitMQ connection has not been established");
		channel.BasicAck(ea.DeliveryTag, false);
	}
}
