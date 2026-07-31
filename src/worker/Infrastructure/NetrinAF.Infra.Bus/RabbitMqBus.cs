using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

        public void Subscribe<T, H>()
            where T : Event
            where H : IEventHandler<T>
        {
            var eventName = typeof(T).Name;

            var handlerType = typeof(H);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException($"O handler {handlerType.Name} já foi registrado no evento {eventName}.");
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            var eventName = typeof(T).Name;
            channel.QueueDeclare(eventName, true, false, false);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Resolve;

            channel.BasicConsume(eventName, false, consumer);
        }

        private async Task Consumer_Resolve(object sender, BasicDeliverEventArgs @event)
        {
            var body = @event.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var eventName = @event.RoutingKey;
            var consumer = (AsyncEventingBasicConsumer)sender;

            try
            {
                // Simulate application business logic processing
                Console.WriteLine($"Processing message: {message}");
                await ProcessEvent(eventName, message);

                // If successful:
                consumer.Model.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}. Moving to DLQ.");

                // CRITICAL: Negative Acknowledge with requeue set to FALSE sends it to the DLX
                consumer.Model.BasicNack(deliveryTag: @event.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subcriptions = _handlers[eventName];
                foreach (var item in subcriptions)
                {
                    var handler = Activator.CreateInstance(item);
                    if (item != null)
                    {
                        var handlerType = _eventTypes.SingleOrDefault(x => x.Name == eventName);
                        var eventData = JsonSerializer.Deserialize(message, handlerType!);
                        var receiver = typeof(IEventHandler<>).MakeGenericType(handlerType!);
                        await (Task)receiver.GetMethod("Handle")!.Invoke(handler, new object[] { eventData! })!;
                    }

                }
            }
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
