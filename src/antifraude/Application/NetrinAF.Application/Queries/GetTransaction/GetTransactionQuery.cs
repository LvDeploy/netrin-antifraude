using NetrinAF.Application.Abstractions.Query;
using NetrinAF.Application.Abstractions.Request;

namespace NetrinAF.Application.Query.GetTransaction
{
    public record GetTransactionQuery : BaseRequest, IQuery<GetTransactionQueryResponse>
    {
        public override bool IsValid() => true;
    }
}
