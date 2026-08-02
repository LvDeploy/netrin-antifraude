using Microsoft.Extensions.DependencyInjection;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NetrinAF.Infra.Bus
{
    public sealed class RabbitMqBus : IEventBus, IDisposable
    {
        private const string DefaultMainExchange = "main.exchange";
        private const string DefaultMainQueueName = "transaction.processing-queue";
        private const string DefaultMainRoutingKey = "transaction.process";
        private const string DefaultDlqName = "transaction.dead-letter-queue";
        private const string DefaultDlqRoutingKey = "transaction.failed";
        private const int DefaultRetryTtlMilliseconds = 60000;
        private const long DefaultMaxRetryAttempts = 1;

        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly Dictionary<string, Type> _eventTypes;
        private readonly RabbitMqSettings _settings;
        private readonly IConnectionFactory _factory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IConnection? _connection;
        private IModel? _channel;
        private bool _isConsuming;

        public RabbitMqBus(RabbitMqSettings settings, IConnectionFactory factory, IServiceScopeFactory serviceScopeFactory)
        {
            _eventTypes = new Dictionary<string, Type>();
            _handlers = new Dictionary<string, List<Type>>();
            _settings = settings;
            _factory = factory;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Subscribe<T, H>()
            where T : Event
            where H : IEventHandler<T>
        {
            string routingKey = _settings.MainRoutingKey ?? DefaultMainRoutingKey;
            var handlerType = typeof(H);

            _eventTypes.TryAdd(routingKey, typeof(T));

            if (!_handlers.ContainsKey(routingKey))
            {
                _handlers.Add(routingKey, new List<Type>());
            }

            if (_handlers[routingKey].Contains(handlerType))
            {
                throw new ArgumentException($"O handler {handlerType.Name} ja foi registrado no evento {routingKey}.");
            }

            _handlers[routingKey].Add(handlerType);
            StartBasicConsume(routingKey);
        }

        private void StartBasicConsume(string routingKey)
        {
            if (_isConsuming)
            {
                return;
            }

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            string mainExchange = _settings.MainExchange ?? DefaultMainExchange;
            string mainQueueName = _settings.MainQueueName ?? DefaultMainQueueName;
            string dlqName = _settings.DlqName ?? DefaultDlqName;
            string dlqRoutingKey = _settings.DlqRoutingKey ?? DefaultDlqRoutingKey;
            int retryTtlMilliseconds = _settings.RetryTtlMilliseconds ?? DefaultRetryTtlMilliseconds;

            var mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", mainExchange },
                { "x-dead-letter-routing-key", dlqRoutingKey }
            };
            var dlqArgs = new Dictionary<string, object>
            {
                { "x-message-ttl", retryTtlMilliseconds },
                { "x-dead-letter-exchange", mainExchange },
                { "x-dead-letter-routing-key", routingKey }
            };

            _channel.ExchangeDeclare(mainExchange, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(dlqName, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs);
            _channel.QueueBind(dlqName, mainExchange, dlqRoutingKey);
            _channel.QueueDeclare(mainQueueName, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs);
            _channel.QueueBind(mainQueueName, mainExchange, routingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Resolve;

            _channel.BasicConsume(mainQueueName, autoAck: false, consumer);
            _isConsuming = true;
        }

        private async Task Consumer_Resolve(object sender, BasicDeliverEventArgs @event)
        {
            var body = @event.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var eventName = @event.RoutingKey;
            var consumer = (AsyncEventingBasicConsumer)sender;

            try
            {
                Console.WriteLine($"Processing message: {message}");
                await ProcessEvent(eventName, message);

                consumer.Model.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                long retryCount = GetQueueDeathCount(@event.BasicProperties.Headers, _settings.DlqName ?? DefaultDlqName);
                long maxRetryAttempts = _settings.MaxRetryAttempts ?? DefaultMaxRetryAttempts;

                if (retryCount >= maxRetryAttempts)
                {
                    Console.WriteLine($"Error encountered after {retryCount} retry attempt(s): {ex.Message}. Message acknowledged and will not be reprocessed.");
                    consumer.Model.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);
                    return;
                }

                Console.WriteLine($"Error encountered: {ex.Message}. Moving to DLQ for retry.");
                consumer.Model.BasicNack(deliveryTag: @event.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (!_handlers.TryGetValue(eventName, out var subscriptions) ||
                !_eventTypes.TryGetValue(eventName, out var eventType))
            {
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();

            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider.GetRequiredService(subscription);
                var eventData = JsonSerializer.Deserialize(message, eventType);
                var receiver = typeof(IEventHandler<>).MakeGenericType(eventType);
                await (Task)receiver.GetMethod("Handler")!.Invoke(handler, new object[] { eventData! })!;
            }
        }

        private static long GetQueueDeathCount(IDictionary<string, object>? headers, string queueName)
        {
            if (headers is null ||
                !headers.TryGetValue("x-death", out var xDeathHeader) ||
                xDeathHeader is not IEnumerable<object> deaths)
            {
                return 0;
            }

            foreach (var death in deaths)
            {
                if (death is not IDictionary<string, object> deathData ||
                    !deathData.TryGetValue("queue", out var queueValue) ||
                    !IsQueueName(queueValue, queueName))
                {
                    continue;
                }

                if (deathData.TryGetValue("count", out var countValue))
                {
                    return Convert.ToInt64(countValue);
                }
            }

            return 0;
        }

        private static bool IsQueueName(object queueValue, string queueName)
        {
            return queueValue switch
            {
                byte[] bytes => Encoding.UTF8.GetString(bytes) == queueName,
                string value => value == queueName,
                _ => queueValue.ToString() == queueName
            };
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
