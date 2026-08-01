using NetrinAF.Application.EventHandlers.TransactionCreatedHandler;
using NetrinAF.Application.Services.TransactionProcess;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events;

namespace AppHost.Registers
{
    internal static class ApplicationRegister
    {
        internal static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<TransactionCreatedHandler>();
            services.AddTransient<IEventHandler<TransactionCreatedEvent>, TransactionCreatedHandler>();
            services.AddScoped<ITransactionProcessService, TransactionProcessService>();
        }

        internal static void UseApplicationServices(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TransactionCreatedEvent, TransactionCreatedHandler>();
        }
    }
}
