using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Infra.Bus;
using NetrinAF.Infra.SQLDatabase.Repository;

namespace NetrinAF.Api.Registers
{
    internal static class InfrastructureRegister
    {
        internal static void AddInfraServices(this IServiceCollection services)
        {
            //Infra.Bus
            services.AddTransient<IEventBus, RabbitMqBus>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<ITransactionHistoricRepository, TransactionHistoricRepository>();
        }
    }
}
