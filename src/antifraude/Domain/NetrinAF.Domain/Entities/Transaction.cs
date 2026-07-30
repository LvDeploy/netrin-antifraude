using NetrinAF.Domain.Entities.Base;
using NetrinAF.Domain.Enums;

namespace NetrinAF.Domain.Entities
{
    public sealed class Transaction : BaseEntity
    {
        protected Transaction(decimal value, TransactionStatus status, DateTime createdAt, string idEmpotency) 
        { 
           Value = value;
           Status = status;
           IdEmpotency = idEmpotency;
           CreatedAt = createdAt;
        }
        public decimal Value { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string IdEmpotency { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Transaction CreateNew(decimal value, string idEmpotency)
        {
            return new Transaction(value: value,
                status: TransactionStatus.REVIEW,
                idEmpotency: idEmpotency,
                createdAt: DateTime.Now);
        }
    }
}
