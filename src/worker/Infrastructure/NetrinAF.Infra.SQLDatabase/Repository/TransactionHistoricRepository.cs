using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Entities;
using NetrinAF.Infra.SQLDatabase.Context;
using NetrinAF.Infra.SQLDatabase.Repository.Base;

namespace NetrinAF.Infra.SQLDatabase.Repository
{
    public class TransactionHistoricRepository : BaseRepository<TransactionHistoric>, ITransactionHistoricRepository
    {
        public TransactionHistoricRepository(EFContext context) : base(context)
        {
        }
    }
}
