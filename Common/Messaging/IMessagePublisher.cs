using RabbitMQ.Client;

namespace Common.Messaging;
public interface IMessagePublisher
{
    void Publish<T>(QueueName queue, T message);
    void Publish<T>(ExchangeName exchange, T message);
}
