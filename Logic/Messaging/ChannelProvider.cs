namespace Logic.Messaging;

using System.Collections.Concurrent;
using System.Configuration;
using Common.Configuration;
using Common.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public sealed class ChannelProvider : IChannelProvider, IDisposable
{
	private IConnection? _connection;
	private readonly ConnectionFactory _connectionFactory;
	private readonly object _connectionLock = new();
	private readonly IDictionary<string, IModel> _channels;
	private readonly ILogger _logger;

	public ChannelProvider(FLAdminConfiguration config, ILogger<ChannelProvider> logger)
	{
		_logger = logger;
		var constructedUri = $"amqp://{config.Messaging.Username}:{config.Messaging.Password}@{config.Messaging.HostName}:{config.Messaging.Port}";
		if (!Uri.TryCreate(constructedUri, UriKind.Absolute, out var uri))
		{
			throw new ConfigurationErrorsException("ConnectionString::Messaging was not found within config.json or it was not a valid URI.");
		}

		_connectionFactory = new ConnectionFactory
		{
			Uri = uri,
			DispatchConsumersAsync = true
		};

		_channels = new ConcurrentDictionary<string, IModel>();
	}

	public IModel? ProvideChannel(string queueName)
	{
		var channel = GetChannel(queueName);
		return channel;
	}

	private IModel? GetChannel(string queueName)
	{
		if (_channels.TryGetValue(queueName, out var channel))
		{
			if (channel is { IsOpen: true })
			{
				return channel;
			}

			_ = _channels.Remove(queueName);
		}

		lock (_connectionLock)
		{
			if (_connection is not { IsOpen: true })
			{
				try
				{
					_connection = _connectionFactory.CreateConnection();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unable to create RabbitMQ connection");
					return null;
				}
			}

			channel = _connection.CreateModel();
		}

		_channels.TryAdd(queueName, channel!);
		return channel!;
	}

	public void Dispose()
	{
		_connection?.Dispose();

		foreach (var channel in _channels.Values)
		{
			channel.Dispose();
		}
	}
}
