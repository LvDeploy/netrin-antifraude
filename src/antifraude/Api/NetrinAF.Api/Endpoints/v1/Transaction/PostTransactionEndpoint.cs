using Microsoft.AspNetCore.Mvc;
using NetrinAF.Api.ApiResults;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Application.Commands.CreateTransaction;
using NetrinAF.Application.Middleware.Correlation;
using Polly.Registry;

namespace NetrinAF.Api.Endpoints.v1.Transaction
{
    public class PostTransactionEndpoint : EndpointBase, IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapPost($"{EndpointTags.Transactions}",
                async ([FromBody] CreateTransactionCommand command,
                CorrelationId correlationId,
                ICommandHandler<CreateTransactionCommand> handler,
                ResiliencePipelineProvider<string> pipelineProvider, 
                CancellationToken cancellationToken
                ) =>
                {
                    var result = await (pipelineProvider.GetPipeline<BaseResponse<Guid>>(ResilienceConstants.BasicCommand))
                          .ExecuteAsync(async x =>
                               await handler.Handle(command, cancellationToken), cancellationToken
                          );

                    return CreateResponse(correlationId.Get(), result);
                })
                .Produces(StatusCodes.Status200OK, typeof(ResultSuccess<Guid>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultFailure))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultFailure))
                .WithTags(EndpointTags.Transactions);
        }
    }
}
