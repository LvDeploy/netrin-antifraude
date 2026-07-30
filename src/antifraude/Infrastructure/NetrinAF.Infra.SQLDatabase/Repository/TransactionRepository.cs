using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Entities;
using NetrinAF.Infra.SQLDatabase.Context;
using NetrinAF.Infra.SQLDatabase.Repository.Base;

namespace NetrinAF.Infra.SQLDatabase.Repository
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(EFContext context) : base(context)
        {
        }
    }
}
