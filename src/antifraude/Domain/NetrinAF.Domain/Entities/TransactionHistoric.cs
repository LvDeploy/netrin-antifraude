using NetrinAF.Domain.Entities.Base;
using NetrinAF.Domain.Enums;

namespace NetrinAF.Domain.Entities
{
    public sealed class TransactionHistoric : BaseEntity
    {
        protected TransactionHistoric(string idEmpotency, TransactionStatus status, DateTime eventTime, Guid transactionId, Transaction transaction, string statusMessage)
        {
            IdEmpotency = idEmpotency;
            Status = status;
            EventTime = eventTime;
            TransactionId = transactionId;
            Transaction = transaction;
            StatusMessage = statusMessage;
        }

        public string IdEmpotency { get; set; }
        public TransactionStatus Status { get; private set; }
        public DateTime EventTime { get; private set; }
        public Guid? TransactionId { get; private set; }
        public Transaction Transaction { get; private set; } = null!;
        public string StatusMessage { get; private set; }

        public TransactionHistoric CreateNew(Transaction transaction, string idEmpotency, TransactionStatus status, string statusMessage )
        {
            return new TransactionHistoric(
                idEmpotency: idEmpotency,
                eventTime: DateTime.Now,
                status: status,
                transaction: transaction,
                transactionId: transaction.Id,
                statusMessage: statusMessage
            );
        }
    }
}
