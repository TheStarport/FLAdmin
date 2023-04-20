using RabbitMQ.Client;

namespace Common.Messaging;
public interface IChannelProvider
{
    public IModel ProvideChannel(string queueName);
}
