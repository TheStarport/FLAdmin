namespace Common.Messaging;
using RabbitMQ.Client;

public interface IChannelProvider
{
	public IModel ProvideChannel(string queueName);
}
