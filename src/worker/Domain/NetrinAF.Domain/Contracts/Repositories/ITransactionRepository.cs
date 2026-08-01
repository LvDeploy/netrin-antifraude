using NetrinAF.Domain.Contracts.Repositories.Base;
using NetrinAF.Domain.Entities;

namespace NetrinAF.Domain.Contracts.Repositories
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<Transaction?> GetWithTrackLog(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Transaction?>> GetByIdEmpotency(string id);
    }
}
