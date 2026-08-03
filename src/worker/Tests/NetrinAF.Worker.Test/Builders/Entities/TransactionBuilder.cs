using NetrinAF.Domain.Entities;
using NetrinAF.Domain.Enums;

namespace NetrinAF.Worker.Test.Builders.Entities;

public sealed class TransactionBuilder
{
    private Guid _id = Guid.NewGuid();
    private decimal _value = 100m;
    private string _idempotencyKey = "idempotency-key";
    private TransactionStatus _status = TransactionStatus.REVIEW;
    private readonly List<string> _trackLogMessages = [];

    public TransactionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

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

    public TransactionBuilder WithStatus(TransactionStatus status)
    {
        _status = status;
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
        transaction.Id = _id;

        if (_status == TransactionStatus.APPROVED)
        {
            transaction.CheckTransaction();
        }
        else if (_status == TransactionStatus.REJECTED)
        {
            transaction.Reject();
        }

        foreach (string statusMessage in _trackLogMessages)
        {
            transaction.TrackLog.Add(TransactionHistoric.CreateNew(transaction, _idempotencyKey, statusMessage));
        }

        return transaction;
    }
}
