using NetrinAF.Domain.Events;

namespace NetrinAF.Worker.Test.Builders.Events;

public sealed class TransactionCreatedEventBuilder
{
    private string _correlationId = "correlation-id";
    private Guid _transactionId = Guid.NewGuid();
    private string _idempotencyKey = "idempotency-key";

    public TransactionCreatedEventBuilder WithCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    public TransactionCreatedEventBuilder WithTransactionId(Guid transactionId)
    {
        _transactionId = transactionId;
        return this;
    }

    public TransactionCreatedEventBuilder WithIdempotencyKey(string idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
        return this;
    }

    public TransactionCreatedEvent Build() => new(_correlationId, _transactionId, _idempotencyKey);
}
