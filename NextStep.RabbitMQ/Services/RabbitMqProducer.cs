using NextStep.RabbitMQ.Interfaces;
using NextStep.RabbitMQ.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace NextStep.RabbitMQ.Services
{
    public class RabbitMqProducer : IRabbitMqProducer, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private RabbitMqProducer(RabbitMqSettings settings, IConnection connection, IChannel channel)
        {
            _settings = settings;
            _connection = connection;
            _channel = channel;
            _settings = settings;

        }
        public static async Task<RabbitMqProducer> CreateAsync(RabbitMqSettings settings)
        {
            var factory = new ConnectionFactory() { HostName = settings.HostName };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                settings.ExchangeName,
                settings.ExchangeType,
                durable: true,
                autoDelete: false
            );

            await channel.QueueDeclareAsync(
                settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueBindAsync(
                settings.QueueName,
                settings.ExchangeName,
                settings.RoutingKey
            );

            return new RabbitMqProducer(settings, connection, channel);
        }

        public async ValueTask DisposeAsync()
        {
            if(_channel != null)
                await _channel.CloseAsync();
            
            if (_connection != null)
                await _connection.CloseAsync();

            GC.SuppressFinalize(this);
        }

        public Task PublishAsync<T>(T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            //var props = _channel.CreateBasicProperties();
            var props = new BasicProperties() { Persistent = true };
            props.Persistent = true;

            _channel.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey,
                mandatory: true,
                basicProperties: props,
                body: body);

            return Task.CompletedTask;
        }
    }
}
