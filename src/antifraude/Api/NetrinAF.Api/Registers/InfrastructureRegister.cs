using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Infra.SQLDatabase.Repository;
using NetrinAF.Infra.SQLDatabase.UnitOfWork;

namespace NetrinAF.Api.Registers
{
    internal static class InfrastructureRegister
    {
        internal static void AddInfraServices(this IServiceCollection services)
        {
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<ITransactionHistoricRepository, TransactionHistoricRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
