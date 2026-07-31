using Microsoft.Extensions.Logging;
using NetrinAF.Application.Abstractions.Command;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Query;
using NetrinAF.Application.Abstractions.Response;
using System.Text.Json;

namespace NetrinAF.Application.Abstractions.Behavior
{
    public static class LoggingDecorator
    {
        public sealed class CommandHandler<TCommand, TResponse>(
            ILogger<ICommandHandler<TCommand, TResponse>> logger,
            ICommandHandler<TCommand, TResponse> innerHandler)
            : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
        {
            public async Task<BaseResponse<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
            {
                string requestName = typeof(TCommand).Name;

                logger.LogInformation($"Processing request {requestName}");

                BaseResponse<TResponse> result = await innerHandler.Handle(command, cancellationToken);

                if (result.IsSuccess)
                {
                    logger.LogInformation($"Completed request {requestName}", JsonSerializer.Serialize(result));
                }
                else
                {
                    logger.LogInformation($"Completed request {requestName} with errors", JsonSerializer.Serialize(result.Errors));
                }

                return result;
            }
        }

        public sealed class CommandHandler<TCommand>(
           ILogger<ICommandHandler<TCommand>> logger,
           ICommandHandler<TCommand> innerHandler)
           : ICommandHandler<TCommand> where TCommand : ICommand
        {
            public async Task<BaseResponse<Guid>> Handle(TCommand command, CancellationToken cancellationToken)
            {
                string requestName = typeof(TCommand).Name;

                logger.LogInformation($"Processing request {requestName}");

                BaseResponse<Guid> result = await innerHandler.Handle(command, cancellationToken);

                if (result.IsSuccess)
                {
                    logger.LogInformation($"Completed request {requestName}", JsonSerializer.Serialize(result));
                }
                else
                {
                    logger.LogInformation($"Completed request {requestName} with errors", JsonSerializer.Serialize(result.Errors));
                }

                return result;
            }
        }

        public sealed class QueryHandler<TQuery, TResponse>(
            ILogger<IQueryHandler<TQuery, TResponse>> logger,
            IQueryHandler<TQuery, TResponse> innerHandler)
            : IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
        {
            public async Task<BaseResponse<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
            {
                string requestName = typeof(TQuery).Name;

                logger.LogInformation($"Processing request {requestName}");

                BaseResponse<TResponse> result = await innerHandler.Handle(query, cancellationToken);

                if(result.IsSuccess)
                {
                    logger.LogInformation($"Completed request {requestName}", JsonSerializer.Serialize(result));
                }
                else
                {
                    logger.LogInformation($"Completed request {requestName} with errors", JsonSerializer.Serialize(result.Errors));
                }

                return result;
            }
        }
    }
}
