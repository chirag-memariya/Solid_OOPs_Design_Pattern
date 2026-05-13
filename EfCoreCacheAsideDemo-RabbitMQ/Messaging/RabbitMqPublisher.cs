using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace EfCoreCacheAsidedemo.Messaging;

    public interface IEventPublisher
    {
        void Publish<T>(T @event);
    }

    public class RabbitMqPublisher : IEventPublisher
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _exchange;
        
        public RabbitMqPublisher(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"],
                UserName = config["RabbitMQ:Username"],
                Password = config["RabbitMQ:Password"]
            };

            _exchange = config["RabbitMQ:Exchange"];
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            
            // Declare exchange once during initialization
            _channel.ExchangeDeclareAsync(
                exchange: _exchange,
                type: ExchangeType.Fanout,
                durable: true).GetAwaiter().GetResult();
        }

        public void Publish<T>(T @event)
        {
            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(@event));

            _channel.BasicPublishAsync(
                exchange: _exchange,
                routingKey: string.Empty,
                body: body).GetAwaiter().GetResult();
        }
    }


// so if we declare exchage once then we dont need to redeclare it in every publish right ?