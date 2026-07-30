using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Infra.SQLDatabase.Context;

namespace NetrinAF.Infra.SQLDatabase.UnitOfWork
{
    public class UnitOfWork(EFContext context) : IUnitOfWork
    {
        public async Task CommitAsync(CancellationToken cancellationToken) 
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
