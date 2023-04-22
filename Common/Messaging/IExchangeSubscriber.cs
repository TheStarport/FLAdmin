namespace Common.Messaging;
using RabbitMQ.Client.Events;

public interface IExchangeSubscriber
{
	string GetQueueName(string exchangeName);
	void Subscribe(string exchangeName, string queueName, AsyncEventHandler<BasicDeliverEventArgs> action);
	void Unsubscribe(string exchangeName);
}
