using NetrinAF.Domain.Entities;

namespace NetrinAF.Test.Builders.Entities;

public sealed class TransactionBuilder
{
    private decimal _value = 100m;
    private string _idempotencyKey = "idempotency-key";
    private readonly List<string> _trackLogMessages = [];

    public TransactionBuilder WithValue(decimal value)
    {
        _value = value;
        return this;
    }

    public TransactionBuilder WithIdempotencyKey(string idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
        return this;
    }

    public TransactionBuilder WithTrackLog(string statusMessage)
    {
        _trackLogMessages.Add(statusMessage);
        return this;
    }

    public Transaction Build()
    {
        Transaction transaction = Transaction.CreateNew(_value, _idempotencyKey);

        foreach (string statusMessage in _trackLogMessages)
        {
            transaction.TrackLog.Add(TransactionHistoric.CreateNew(transaction, _idempotencyKey, statusMessage));
        }

        return transaction;
    }
}
