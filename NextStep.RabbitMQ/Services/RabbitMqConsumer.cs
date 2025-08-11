using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NextStep.RabbitMQ.Models;
using NextStep.RabbitMQ.Interfaces;

namespace NextStep.RabbitMQ.Services;

public class RabbitMqConsumer : IRabbitMqConsumer
{
    private readonly RabbitMqSettings _settings;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqConsumer(RabbitMqSettings settings, IConnection connection, IChannel channel)
    {
        _settings = settings;
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqConsumer> CreateAsync(RabbitMqSettings settings)
    {
        var factory = new ConnectionFactory() { HostName = settings.HostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            settings.ExchangeName,
            settings.ExchangeType,
            durable: true,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            settings.QueueName,
            settings.ExchangeName,
            settings.RoutingKey);

        return new RabbitMqConsumer(settings, connection, channel);
    }

    public async Task StartConsumingAsync<T>(Func<T, Task<bool>> onMessageReceived, CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var deserialized = JsonSerializer.Deserialize<T>(message);
                bool success = await onMessageReceived(deserialized);

                if (success)
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                else
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
            }
            catch
            {
                await _channel.BasicRejectAsync(ea.DeliveryTag, false, cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(_settings.QueueName, autoAck: false, consumer, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();

        GC.SuppressFinalize(this);
    }
}