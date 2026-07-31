using NetrinAF.Domain.Events.Base;

namespace NetrinAF.Domain.Events
{
    public class TransactionEvent : Event
    {
        public Guid Id { get; set; }

        public TransactionEvent(string correlationId, Guid id)
        {
            CorrelationId = correlationId;
            Id = id;
        }
    }
}
