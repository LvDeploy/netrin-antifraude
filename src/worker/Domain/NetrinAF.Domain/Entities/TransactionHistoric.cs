using NetrinAF.Domain.Entities.Base;
using NetrinAF.Domain.Enums;

namespace NetrinAF.Domain.Entities
{
    public sealed class TransactionHistoric : BaseEntity
    {
        private TransactionHistoric()
        {
            IdEmpotency = string.Empty;
            StatusMessage = string.Empty;
        }

        protected TransactionHistoric(string idEmpotency, TransactionStatus status, DateTime eventTime, Guid transactionId, Transaction transaction, string statusMessage)
        {
            Id = Guid.NewGuid();
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
        public Guid TransactionId { get; private set; }
        public Transaction Transaction { get; private set; } = null!;
        public string StatusMessage { get; private set; }

        public static TransactionHistoric CreateNew(Transaction transaction, string idEmpotency, string statusMessage )
        {
            return new TransactionHistoric(
                idEmpotency: idEmpotency,
                eventTime: DateTime.Now,
                status: transaction.Status,
                transaction: transaction,
                transactionId: transaction.Id,
                statusMessage: statusMessage
            );
        }
    }
}
