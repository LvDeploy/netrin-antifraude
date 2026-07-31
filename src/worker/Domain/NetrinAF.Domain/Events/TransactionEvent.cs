using NetrinAF.Domain.Events.Base;

namespace NetrinAF.Domain.Events
{
    public class TransactionEvent : Event
    {
        public Guid TransactionId { get; set; }

        public TransactionEvent(string correlationId, Guid transactionId)
        {
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }
    }
}
