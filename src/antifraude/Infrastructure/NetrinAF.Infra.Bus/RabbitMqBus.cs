using NetrinAF.Domain.Bus;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace NetrinAF.Infra.Bus
{
    public sealed class RabbitMqBus : IEventBus
    {
        private const string DefaultMainQueueName = "transaction.processing-queue";
        private const string DefaultMainRoutingKey = "transaction.process";

        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly RabbitMqSettings _settings;
        private readonly IConnectionFactory _factory;

        public RabbitMqBus(RabbitMqSettings settings, IConnectionFactory factory)
        {
            _eventTypes = new List<Type>();
            _handlers = new Dictionary<string, List<Type>>();
            _settings = settings;
            _factory = factory;
            Setup();
        }

        public void Setup()
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            string mainExchange = _settings.MainExchange!;
            string mainQueueName = _settings.MainQueueName ?? DefaultMainQueueName;
            string mainRoutingKey = _settings.MainRoutingKey ?? DefaultMainRoutingKey;

            var mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", mainExchange },
                { "x-dead-letter-routing-key", _settings.DlqRoutingKey! }
            };

            if (!DoesExchangeExist(connection, mainExchange))
            {
                channel.ExchangeDeclare(mainExchange, ExchangeType.Direct, durable: true);
            }

            if (!DoesQueueExist(connection, _settings.DlqName!))
            {
                channel.QueueDeclare(_settings.DlqName!, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(_settings.DlqName!, mainExchange, _settings.DlqRoutingKey!);
            }
            if (!DoesQueueExist(connection, mainQueueName))
            {
                channel.QueueDeclare(
                    queue: mainQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: mainQueueArgs
                );
            }

            channel.QueueBind(mainQueueName, mainExchange, mainRoutingKey);
           
        }

        public void Publish<T>(T @event)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            string mainExchange = _settings.MainExchange!;
            string mainRoutingKey = _settings.MainRoutingKey ?? DefaultMainRoutingKey;

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(mainExchange, mainRoutingKey, properties, body);
        }

        private bool DoesQueueExist(IConnection connection, string queueName)
        {
            using var channel = connection.CreateModel();
            try
            {
                channel.QueueDeclarePassive(queueName);
                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                return false;
            }
        }

        private bool DoesExchangeExist(IConnection connection, string exchangeName)
        {
            using var channel = connection.CreateModel();
            try
            {
                channel.ExchangeDeclarePassive(exchangeName);
                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                return false;
            }
        }
    }
}
