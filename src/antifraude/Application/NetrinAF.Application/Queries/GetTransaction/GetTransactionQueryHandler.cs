using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;

namespace NetrinAF.Application.Query.GetTransaction
{
    public sealed class GetTransactionQueryHandler() : IQueryHandler<GetTransactionQuery, GetTransactionQueryResponse>
    {
        public async Task<BaseResponse<GetTransactionQueryResponse>> Handle(GetTransactionQuery query, CancellationToken cancellationToken)
        {


            return ResponseBuilder.Success(new GetTransactionQueryResponse());
        }
    }
}
