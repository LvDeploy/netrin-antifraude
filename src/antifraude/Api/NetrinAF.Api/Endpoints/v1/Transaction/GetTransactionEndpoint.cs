using Microsoft.AspNetCore.Mvc;
using NetrinAF.Api.ApiResults;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Application.Query.GetTransaction;

namespace NetrinAF.Api.Endpoints.v1.Transaction
{
    public class GetTransactionEndpoint : EndpointBase, IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet($"{EndpointTags.Transactions}/{{id}}",
                async ([FromRoute] Guid id,
                CorrelationId correlationId,
                IQueryHandler<GetTransactionQuery, GetTransactionQueryResponse> handler,
                CancellationToken cancellationToken
                ) =>
                {
                    var result = await handler.Handle(new GetTransactionQuery(id), cancellationToken);

                    return CreateResponse(correlationId.Get(), result);
                })
                .Produces(StatusCodes.Status200OK, typeof(ResultSuccess<GetTransactionQueryResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultFailure))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultFailure))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultFailure))
                .WithTags(EndpointTags.Transactions);
        }
    }
}
