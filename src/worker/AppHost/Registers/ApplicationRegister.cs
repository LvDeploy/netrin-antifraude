using NetrinAF.Application.Handlers.TransactionEventHandler;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events;

namespace AppHost.Registers
{
    internal static class ApplicationRegister
    {
        internal static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IEventHandler<TransactionEvent>, TransactionEventHandler>();
        }

        internal static void UseApplicationServices(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TransactionEvent, TransactionEventHandler>();
        }
    }
}
