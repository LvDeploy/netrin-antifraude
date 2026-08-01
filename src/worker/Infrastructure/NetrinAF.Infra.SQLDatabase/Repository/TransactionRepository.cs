using Microsoft.EntityFrameworkCore;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Entities;
using NetrinAF.Infra.SQLDatabase.Context;
using NetrinAF.Infra.SQLDatabase.Repository.Base;

namespace NetrinAF.Infra.SQLDatabase.Repository
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        private readonly EFContext _context;

        public TransactionRepository(EFContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetWithTrackLog(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Transactions
                .AsNoTracking()
                .AsSingleQuery()
                .Include(transaction => transaction.TrackLog)
                .SingleOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Transaction?>> GetByIdEmpotency(string id)
        {
            return await _context.Transactions                
                .AsSingleQuery()
                .Where(transaction => transaction.IdEmpotency == id).ToListAsync();
        }
    }
}
