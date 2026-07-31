using NetrinAF.Application.Abstractions.Behavior;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Application.Middleware.Idempotency;
using System.Reflection;

namespace NetrinAF.Api.Registers
{
    internal static class ApplicationRegister
    {
        internal static void AddApplicationServices(this IServiceCollection services, string applicationName)
        {
            services.AddScoped<CorrelationId>();
            services.AddScoped<IdempotencyKey>();

            Assembly assemblyApplication = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name.Equals(applicationName, StringComparison.OrdinalIgnoreCase))!;

            services.Scan(scan => scan.FromAssemblies(assemblyApplication)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                 .AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                 .AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                 .AsImplementedInterfaces().WithScopedLifetime()
            );
            //Attached with non-generic overloads
            services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
            services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandler<>));

        }
    }
}
