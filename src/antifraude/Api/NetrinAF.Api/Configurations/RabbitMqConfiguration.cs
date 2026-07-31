using Microsoft.EntityFrameworkCore;
using NetrinAF.Infra.Bus;
using NetrinAF.Infra.SQLDatabase.Context;
using RabbitMQ.Client;

namespace NetrinAF.Api.Configurations
{
    internal static class RabbitMqConfiguration
    {
        internal static void AddRabbitMqConfiguration(this WebApplicationBuilder builder)
        {
            RabbitMqSettings options = builder.Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>()!;
            builder.Services.AddSingleton(options);

            builder.Services.AddSingleton<IConnectionFactory>(sp =>
            {
                return new ConnectionFactory
                {
                    HostName = builder.Configuration["RabbitMqSettings:DefaultHost"] ?? "localhost",
                    UserName = builder.Configuration["RabbitMqSettings:UserName"] ?? "guest",
                    Password = builder.Configuration["RabbitMqSettings:Password"] ?? "guest",
                    DispatchConsumersAsync = true
                };
            });
        }
    }
}
