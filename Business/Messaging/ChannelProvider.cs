using System.Collections.Concurrent;
using System.Configuration;
using Common.Messaging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Business.Messaging;
public sealed class ChannelProvider : IChannelProvider, IDisposable
{
    private IConnection? _connection;
    private readonly ConnectionFactory _connectionFactory;
    private readonly object _connectionLock = new();
    private readonly IDictionary<string, IModel> _channels;

    public ChannelProvider(IConfiguration configuration)
    {
        if (!Uri.TryCreate(configuration.GetConnectionString("Messaging"), UriKind.Absolute, out Uri? uri))
        {
            throw new ConfigurationErrorsException("ConnectionString::Messaging was not found within appsettings.json");
        }

        _connectionFactory = new ConnectionFactory
        {
            Uri = uri,
            DispatchConsumersAsync = true
        };

        _channels = new ConcurrentDictionary<string, IModel>();
    }

    public IModel ProvideChannel(string queueName)
    {
        try
        {
            var channel = GetChannel(queueName);
            return channel!;
        }
        catch
        {
            throw;
        }
    }

    private IModel GetChannel(string queueName)
    {
        if (_channels.TryGetValue(queueName, out var channel))
        {
            if (channel is { IsOpen: true })
            {
                return channel;
            }

            _channels.Remove(queueName);
        }

        lock (_connectionLock)
        {
            if (_connection is not { IsOpen: true })
            {
                _connection = _connectionFactory.CreateConnection();
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
