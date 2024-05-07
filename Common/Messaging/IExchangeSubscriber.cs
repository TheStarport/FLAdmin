namespace Common.Messaging;

using RabbitMQ.Client.Events;

public interface IExchangeSubscriber
{
	string GetQueueName(ExchangeName name);
	void Subscribe(ExchangeName name, string queueName, AsyncEventHandler<BasicDeliverEventArgs> action);
	void EnsureDeclared(ExchangeName name, string type, bool durable = true);
	void Unsubscribe(ExchangeName name);
}
