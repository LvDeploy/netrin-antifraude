using Microsoft.AspNetCore.Mvc;
using NetrinAF.Api.ApiResults;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Commands.CreateTransaction;
using NetrinAF.Application.Middleware.Correlation;

namespace NetrinAF.Api.Endpoints.Transaction
{
    public class PostTransactionEndpoint : EndpointBase, IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapPost("transactions",
                async ([FromBody] CreateTransactionCommand command,
                CorrelationId correlationId,
                ICommandHandler<CreateTransactionCommand> handler,
                CancellationToken cancellationToken
                ) =>
                {
                    var result = await handler.Handle(command, cancellationToken);
                    return CreateResponse(correlationId.Get(), result);
                })
                .Produces(StatusCodes.Status200OK, typeof(ResultSuccess<Guid>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultFailure))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultFailure))
                .WithTags(EndpointTags.Transactions);
        }
    }
}
