using NetrinAF.Domain.Bus;
using NetrinAF.Infra.Bus;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;

namespace AppHost.Configurations
{
    [ExcludeFromCodeCoverage]
    internal static class RabbitMqConfiguration
    {
        internal static void AddRabbitMqConfiguration(this WebApplicationBuilder builder)
        {
            RabbitMqSettings options = builder.Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>() ?? new RabbitMqSettings();
            builder.Services.AddSingleton(options);

            builder.Services.AddSingleton<IConnectionFactory>(sp =>
            {
                return new ConnectionFactory
                {
                    HostName = options.DefaultHost ?? "localhost",
                    UserName = options.UserName ?? "guest",
                    Password = options.Password ?? "guest",
                    DispatchConsumersAsync = true
                };
            });

            builder.Services.AddSingleton<IEventBus, RabbitMqBus>();
        }
    }
}
