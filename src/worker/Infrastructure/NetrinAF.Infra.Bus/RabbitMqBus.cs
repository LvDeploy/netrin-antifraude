using Microsoft.Extensions.DependencyInjection;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NetrinAF.Infra.Bus
{
    public sealed class RabbitMqBus : IEventBus
    {
        private const string DefaultMainQueueName = "transaction.processing-queue";
        private const string DefaultMainRoutingKey = "transaction.proccess";

        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly RabbitMqSettings _settings;
        private readonly IConnectionFactory _factory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public RabbitMqBus(RabbitMqSettings settings, IConnectionFactory factory, IServiceScopeFactory serviceScopeFactory)
        {
            _eventTypes = new List<Type>();
            _handlers = new Dictionary<string, List<Type>>();
            _settings = settings;
            _factory = factory;
            _serviceScopeFactory = serviceScopeFactory;
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
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subcriptions = _handlers[eventName];
                    foreach (var item in subcriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription);
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
        }
    }
}
