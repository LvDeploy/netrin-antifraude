using NetrinAF.Application.Abstractions.Query;
using NetrinAF.Application.Abstractions.Response;

namespace NetrinAF.Application.Abstractions.Handler
{
    public interface IQueryHandler<in TQuery, TResponse>
        where  TQuery : IQuery<TResponse>
    {
        Task<BaseResponse<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
