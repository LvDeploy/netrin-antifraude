using NetrinAF.Application.Query.GetTransaction;

namespace NetrinAF.Test.Builders.Queries;

public sealed class GetTransactionQueryBuilder
{
    private Guid _id = Guid.NewGuid();

    public GetTransactionQueryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public GetTransactionQuery Build() => new(_id);
}
