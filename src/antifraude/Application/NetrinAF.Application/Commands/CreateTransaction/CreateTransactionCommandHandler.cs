using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events;

namespace NetrinAF.Application.Commands.CreateTransaction
{
    public sealed class CreateTransactionCommandHandler(IEventBus bus, CorrelationId correlationId) : ICommandHandler<CreateTransactionCommand>
    {
        public async Task<BaseResponse<Guid>> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            bus.Publish(new TransactionEvent(correlationId.Get(), Guid.NewGuid()));

            return  ResponseBuilder.Success(Guid.Empty);
        }
    }
}
