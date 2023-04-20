﻿using RabbitMQ.Client.Events;

namespace Common.Messaging;
public interface IMessageSubscriber
{
    void Complete(string queueName, BasicDeliverEventArgs ea);
    void Reject(string queueName, BasicDeliverEventArgs ea, bool requeue);
    void Subscribe(string queueName, AsyncEventHandler<BasicDeliverEventArgs> action);
    void Unsubscribe(string queueName);
}
