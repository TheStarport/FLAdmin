using RabbitMQ.Client.Events;

namespace FlAdmin.Common.DataAccess.Messaging;

public interface IMessageSubscriber
{
    void Complete(string queueName, BasicDeliverEventArgs eventArgs);
    void Reject(string queueName, BasicDeliverEventArgs eventArgs, bool requeue);
    void Subscribe(string queueName, AsyncEventHandler<BasicDeliverEventArgs> action);
    void Unsubscribe(string queueName);
}