using NetrinAF.Domain.Events.Base;

namespace NetrinAF.Domain.Events
{
    public class TransactionCreatedEvent : Event
    {
        public Guid TransactionId { get; set; }
        public string IdempotencyKey { get; set; }

        public TransactionCreatedEvent()
        {
            CorrelationId = string.Empty;
            IdempotencyKey = string.Empty;
        }

        public TransactionCreatedEvent(string correlationId, Guid transactionId, string idempotencyKey)
        {
            CorrelationId = correlationId;
            TransactionId = transactionId;
            IdempotencyKey = idempotencyKey;
        }
    }
}
